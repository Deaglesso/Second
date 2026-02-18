using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Second.Application.Contracts.Services;
using Second.Application.Exceptions;
using Second.Domain.Entities;

namespace Second.Persistence.Implementations.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
        {
            var key = _configuration["Jwt:Key"] ?? throw new ConfigurationAppException("Missing Jwt:Key configuration.");
            var issuer = _configuration["Jwt:Issuer"] ?? "Second.API";
            var audience = _configuration["Jwt:Audience"] ?? "Second.Client";
            var expiresInMinutes = int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out var parsed) ? parsed : 60;

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }
    }
}
