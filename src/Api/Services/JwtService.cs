namespace Api.Service;

using Api.Configuration;
using Api.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService(IOptions<JwtOptions> configuration, ILogger<JwtService> logger)
{
    public string GenerateToken(GoogleUserInfo userInfo)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Value.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        logger.LogInformation("{} {} {}", userInfo.Email, userInfo.Name, userInfo.Sub);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userInfo.Name),
            new Claim("sub", userInfo.Sub),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration.Value.Issuer,
            audience: configuration.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
