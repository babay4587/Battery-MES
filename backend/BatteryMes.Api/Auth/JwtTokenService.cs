using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BatteryMes.Api.Auth;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public string CreateToken(string userId, string loginName, string displayName, IReadOnlyList<string> roles)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "battery-mes";
        var audience = configuration["Jwt:Audience"] ?? "battery-mes";
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("缺少 Jwt:Key");
        var minutes = int.TryParse(configuration["Jwt:ExpireMinutes"], out var parsed) ? parsed : 720;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, loginName),
            new(ClaimTypes.Name, displayName),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
