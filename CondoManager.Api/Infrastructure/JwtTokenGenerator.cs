using CondoManager.Entity.Models;
using CondoManager.Entity.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CondoManager.Api.Config;

namespace CondoManager.Api.Infrastructure
{
    public class JwtTokenGenerator
    {
        private readonly JwtOptions _options;

        public JwtTokenGenerator(JwtOptions options)
        {
            _options = options;
        }

        public string Generate(User user)
        {
            return GenerateToken(user.Id, user.Email, user.Roles);
        }

        public string GenerateToken(int userId, string? email, UserRole roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email ?? "")
            };

            // Add individual role claims for proper authorization
            if (roles.HasFlag(UserRole.Admin))
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            if (roles.HasFlag(UserRole.Resident))
                claims.Add(new Claim(ClaimTypes.Role, "Resident"));
            if (roles.HasFlag(UserRole.Trustee))
                claims.Add(new Claim(ClaimTypes.Role, "Trustee"));
            if (roles.HasFlag(UserRole.Gatekeeper))
                claims.Add(new Claim(ClaimTypes.Role, "Gatekeeper"));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
