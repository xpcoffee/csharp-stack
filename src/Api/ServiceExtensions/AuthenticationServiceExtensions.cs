namespace Api.ServiceExtensions;

using System.Text;
using Api.Configuration;
using Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationWebApplicationExtensions
{
    public static WebApplicationBuilder AddGoogleAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<GoogleAuthOptions>()
            .Bind(builder.Configuration.GetSection(GoogleAuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtOptions = builder.Configuration.GetRequiredSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidDataException("JwtOptions could not be pulled from configuration");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key))
                };
            });

        builder.Services
              .AddScoped<GoogleAuthService>()
              .AddScoped<JwtService>()
              .AddAuthorization();

        return builder;
    }
}
