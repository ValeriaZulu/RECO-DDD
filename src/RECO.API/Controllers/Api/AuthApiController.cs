using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RECO.Domain.Interfaces;
using RECO.API.Services;
using RECO.Domain.Entities;
using BCrypt.Net;

namespace RECO.API.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly JwtService _jwt;

        public AuthApiController(IUserRepository users, JwtService jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password)) return BadRequest("email and password required");
            var existing = await _users.GetByEmailAsync(req.Email);
            if (existing != null) return Conflict("email already registered");

            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            var user = new User(Guid.NewGuid(), req.Email, hash, req.Name);
            // create profile
            var profile = new Profile(Guid.NewGuid(), user.Id);
            user.SetProfile(profile);

            await _users.AddAsync(user);

            var token = _jwt.GenerateToken(user.Id, user.Email, user.DisplayName);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password)) return BadRequest("email and password required");
            var user = await _users.GetByEmailAsync(req.Email);
            if (user == null) return Unauthorized("invalid credentials");
            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return Unauthorized("invalid credentials");

            var token = _jwt.GenerateToken(user.Id, user.Email, user.DisplayName);
            return Ok(new { token });
        }
    }

    public record RegisterRequest(string Name, string Email, string Password);
    public record LoginRequest(string Email, string Password);
}
