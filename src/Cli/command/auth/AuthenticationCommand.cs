using System;
using System.Threading.Tasks;
using Cli.Api.Helpers;
using Cli.Authentication.Api;
using Cli.Authentication.Google;
using CommandLine;
using Microsoft.Extensions.Configuration;

namespace Cli.Commands.Authentication;

public class AuthenticationCommand
{
    private static readonly string[] Scopes = { "openid", "email", "profile" };

    public static async Task<int> Handle(AuthOptions options, IConfigurationRoot configuration)
    {
        try
        {
            var baseUrl = ApiHelpers.GetBaseUrl(options.Endpoint);

            var googleAuthConfig = new GoogleAuthConfig();
            configuration.GetSection(GoogleAuthConfig.ConfigSectionName).Bind(googleAuthConfig);

            // Step 1: Get Google ID Token
            string idToken = await new GoogleDeviceFlowAuth(
                googleAuthConfig.ClientId,
                googleAuthConfig.ClientSecret
                ).AuthenticateAsync(Scopes);
            Console.WriteLine("✓ Google ID Token obtained");

            // Step 2: Exchange ID Token for JWT
            string jwt = await ApiAuthentication.ExchangeIdTokenForJwtAsync(baseUrl, idToken);
            if (jwt is null)
            {
                throw new Exception("JWT is empty");
            }

            Console.WriteLine("✓ JWT obtained from API");
            ApiAuthentication.SaveJwtToFile(jwt);
            Console.WriteLine("✓ JWT cached to file");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Issue authenticating: {ex}");
            return 1;
        }
    }
}

[Verb("authenticate", HelpText = "Authenticate with Google Auth")]
public class AuthOptions : BaseCommand { }

