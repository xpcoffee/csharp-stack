using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    [Required(ErrorMessage = "Google ClientId is required")]
    public string ClientId { get; set; } = string.Empty;
}

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT Key is required")]
    [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters long")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 24 * 7, ErrorMessage = "Expiration hours must be between 1 and 168 (7 days)")]
    public int ExpirationHours { get; set; } = 24;
}

