using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RECO.Infrastructure.Persistence;
using RECO.Domain.Entities;

namespace RECO.API.Controllers.Api
{
    [ApiController]
    [Route("api/preferences")]
    public class PreferencesApiController : ControllerBase
    {
        private readonly RECODbContext _db;

        public PreferencesApiController(RECODbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Save([FromBody] PreferencesRequest req)
        {
            var sub = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (!System.Guid.TryParse(sub, out var userId)) return Unauthorized();

            // find profile for user
            var profile = await _db.Profiles.FindAsync(new object[] { userId });
            // The Profile primary key is a Guid Id, not UserId. So lookup by UserId
            if (profile == null)
            {
                profile = _db.Profiles.FirstOrDefault(p => p.UserId == userId);
            }
            if (profile == null)
            {
                // create if missing
                profile = new Profile(System.Guid.NewGuid(), userId);
                _db.Profiles.Add(profile);
            }

            // Clear existing genre preferences
            profile.GenrePreferences.Clear();

            if (req.GenreIds != null)
            {
                foreach (var gid in req.GenreIds)
                {
                    var g = await _db.Genres.FindAsync(gid);
                    if (g != null)
                    {
                        profile.AddGenrePreference(new GenrePreference(g.Id, g.Name));
                    }
                }
            }

            await _db.SaveChangesAsync();

            return Ok();
        }
    }

    public class PreferencesRequest { public int[]? GenreIds { get; set; } }
}
