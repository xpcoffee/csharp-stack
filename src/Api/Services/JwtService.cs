namespace Api.Service;

using Api.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService(IConfiguration configuration, ILogger<JwtService> logger)
{
    private readonly IConfiguration _configuration = configuration;

    public string GenerateToken(GoogleUserInfo userInfo)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidDataException("Jwt:Key not found");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        logger.LogInformation("{} {} {}", userInfo.Email, userInfo.Name, userInfo.Sub);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userInfo.Name),
            new Claim("sub", userInfo.Sub),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
