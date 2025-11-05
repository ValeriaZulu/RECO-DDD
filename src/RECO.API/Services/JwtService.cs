using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace RECO.API.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration cfg)
        {
            _secret = cfg["JWT_SECRET"] ?? Environment.GetEnvironmentVariable("JWT_SECRET") ?? "RECO_SUPER_SECURE_KEY_2025_987654321!";
            if (!int.TryParse(cfg["JWT_EXP_MINUTES"], out _expiryMinutes)) _expiryMinutes = 60 * 24;
        }

        public string GenerateToken(Guid userId, string email, string? displayName = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),
                new Claim("name", displayName ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
