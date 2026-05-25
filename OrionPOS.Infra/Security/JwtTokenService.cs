using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrionPOS.Application.Auth;
using OrionPOS.Domain.Auth;

namespace OrionPOS.Infra.Security;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string GenerateAccessToken(User user)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "orionpos";
        var audience = configuration["Jwt:Audience"] ?? "orionpos-app";
        var key = configuration["Jwt:Key"] ?? "development-super-secret-key-change-me";
        var expiresInMinutes = int.TryParse(configuration["Jwt:ExpiresInMinutes"], out var minutes) ? minutes : 60;

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
