namespace Api.Service;

using Api.Configuration;
using Api.Model;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

public class GoogleAuthService(IOptions<GoogleAuthOptions> configuration)
{
    public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { configuration.Value.ClientId }
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
