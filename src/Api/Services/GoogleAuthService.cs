namespace Api.Service;

using Api.Model;
using Google.Apis.Auth;

public class GoogleAuthService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
    {
        var clientId = _configuration["GoogleAuth:ClientId"] ?? throw new InvalidDataException("GoogleAuth:ClientId is not in config");

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            return new GoogleUserInfo
            {
                Email = payload.Email,
                Name = payload.Name,
                Picture = payload.Picture,
                Sub = payload.Subject
            };
        }
        catch (Exception)
        {
            return null; // Invalid token
        }
    }
}
