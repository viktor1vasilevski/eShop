using eShop.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace eShop.Application.Helpers;

public static class JwtTokenHelper
{
    public static string GenerateToken(IConfiguration configuration, User user)
    {
        var secretKey = configuration["JwtSettings:Secret"] ?? "AlternativeSecretKeyOfAtLeast32Characters!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.Name, user.Username)
            };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(22),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
