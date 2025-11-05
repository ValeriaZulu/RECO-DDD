using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RECO.Domain.Interfaces;
using RECO.API.Services;
using RECO.Infrastructure.Persistence;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace RECO.API.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IUserRepository _users;
        private readonly RECODbContext _db;
        private readonly JwtService _jwt;

        public AuthController(IUserRepository users, RECODbContext db, JwtService jwt)
        {
            _users = users;
            _db = db;
            _jwt = jwt;
        }

        [HttpGet("/login")]
        public IActionResult Login() => View("~/Views/Auth/Login.cshtml");

        [HttpPost("/login")]
        public async Task<IActionResult> LoginPost(string email, string password)
        {
            var user = await _users.GetByEmailAsync(email);
            if (user == null) { ModelState.AddModelError("", "Invalid credentials"); return View("~/Views/Auth/Login.cshtml"); }
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) { ModelState.AddModelError("", "Invalid credentials"); return View("~/Views/Auth/Login.cshtml"); }

            var token = _jwt.GenerateToken(user.Id, user.Email, user.DisplayName);
            Response.Cookies.Append("jwt", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Cambia a true solo si usas HTTPS estable
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Path = "/"
            });
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/register")]
        public IActionResult Register() => View("~/Views/Auth/Register.cshtml");

        [HttpPost("/register")]
        public async Task<IActionResult> RegisterPost(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) { ModelState.AddModelError("", "Email and password required"); return View("~/Views/Auth/Register.cshtml"); }
            var existing = await _users.GetByEmailAsync(email);
            if (existing != null) { ModelState.AddModelError("", "Email already registered"); return View("~/Views/Auth/Register.cshtml"); }

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new RECO.Domain.Entities.User(Guid.NewGuid(), email, hash, name);
            var profile = new RECO.Domain.Entities.Profile(Guid.NewGuid(), user.Id);
            user.SetProfile(profile);
            await _users.AddAsync(user);

            var token = _jwt.GenerateToken(user.Id, user.Email, user.DisplayName);
            Response.Cookies.Append("jwt", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Cambia a true solo si usas HTTPS estable
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Path = "/"
            });

            return RedirectToAction("Preferences");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            // Remove the JWT cookie and redirect to login
            if (Request.Cookies.ContainsKey("jwt"))
            {
                Response.Cookies.Delete("jwt");
            }
            return RedirectToAction("Login");
        }

        [HttpGet("/preferences")]
        public async Task<IActionResult> Preferences()
        {
            var genres = await _db.Genres.ToListAsync();
            return View("~/Views/Auth/Preferences.cshtml", genres);
        }

        [HttpPost("/preferences")]
        public async Task<IActionResult> PreferencesPost(int[] selectedGenreIds)
        {
            // read jwt to get user id
            var token = Request.Cookies["jwt"];
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login");
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var sub = jwt.Subject;
            if (!Guid.TryParse(sub, out var userId)) return RedirectToAction("Login");

            var profile = _db.Profiles.FirstOrDefault(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new RECO.Domain.Entities.Profile(Guid.NewGuid(), userId);
                _db.Profiles.Add(profile);
            }
            profile.GenrePreferences.Clear();
            foreach (var gid in selectedGenreIds ?? Array.Empty<int>())
            {
                var g = await _db.Genres.FindAsync(gid);
                if (g != null) profile.AddGenrePreference(new RECO.Domain.Entities.GenrePreference(g.Id, g.Name));
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
