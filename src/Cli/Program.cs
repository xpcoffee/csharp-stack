using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;

namespace Cli.Program;

public class Program
{
    private static readonly string[] Scopes = { "openid", "email", "profile" };
    private static readonly string ApplicationName = "C# stack cli";

    // Replace with your API endpoint
    private const string TokenExchangePath = "/api/Auth/google";

    [Verb("auth", HelpText = "Authentication commands")]
    public class AuthOptions
    {
        [Option('e', "endpoint", Required = false, HelpText = "The API endpoint e.g. http://localhost:5001")]
        public string Endpoint { get; set; } = string.Empty;
    }

    [Verb("user", HelpText = "Actions on users")]
    public class UserOptions
    {
        [Option('e', "endpoint", Required = false, HelpText = "The API endpoint e.g. http://localhost:5001")]
        public string Endpoint { get; set; } = string.Empty;

        [Option('t', "token", Required = false, HelpText = "The JWT for authentication")]
        public string Token { get; set; } = string.Empty;
    }

    public class GoogleAuthConfig
    {
        public static readonly string ConfigSectionName = "GoogleAuth";
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }


    public static async Task Main(string[] args)
    {
        try
        {
            DotNetEnv.Env.Load();
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddEnvironmentVariables().Build();

            Console.WriteLine(ApplicationName);
            Console.WriteLine("========================");

            var result = await Parser.Default.ParseArguments<AuthOptions, UserOptions>(args)
                   .MapResult(
                       (AuthOptions opts) => HandleAuthCommand(opts, configuration),
                       (UserOptions opts) => HandleUserCommand(opts, configuration),
                       errs => Task.FromResult(1));

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task<int> HandleAuthCommand(AuthOptions options, IConfigurationRoot configuration)
    {
        var baseUrl = GetBaseUrl(options.Endpoint);
        var jwt = await Authenticate(baseUrl, configuration);
        if (jwt is not null)
        {

            Console.WriteLine(jwt);
            return 0;
        }
        Console.Error.WriteLine("JWT is empty");
        return 1;
    }

    private static async Task<int> HandleUserCommand(UserOptions options, IConfigurationRoot configuration)
    {
        try
        {
            var baseUrl = GetBaseUrl(options.Endpoint);

            await ListAllUsers(baseUrl, options.Token);

            Console.WriteLine("✓ Call successful");
            return 0;
        }
        catch
        {
            return 1;
        }

    }

    private static string GetBaseUrl(string endpoint)
    {
        var builder = new UriBuilder(endpoint);
        return $"{builder.Scheme}://{builder.Host}:{builder.Port}";
    }

    private static async Task<string> Authenticate(string apiBaseUrl, IConfigurationRoot configuration)
    {
        var googleAuthConfig = new GoogleAuthConfig();
        configuration.GetSection(GoogleAuthConfig.ConfigSectionName).Bind(googleAuthConfig);

        // Step 1: Get Google ID Token
        string idToken = await GetGoogleIdTokenAsync(googleAuthConfig);
        Console.WriteLine("✓ Google ID Token obtained");

        // Step 2: Exchange ID Token for JWT
        string jwt = await ExchangeIdTokenForJwtAsync(apiBaseUrl, idToken);
        Console.WriteLine("✓ JWT token obtained from API");

        Console.WriteLine("\nAuthentication flow completed successfully!");

        return jwt;
    }

    private static async Task<string> GetGoogleIdTokenAsync(GoogleAuthConfig authConfig)
    {
        var deviceFlow = new GoogleDeviceFlowAuth(authConfig.ClientId, authConfig.ClientSecret);
        return await deviceFlow.AuthenticateAsync(Scopes);
    }

    private static async Task<string> ExchangeIdTokenForJwtAsync(string apiBaseUrl, string idToken)
    {
        using var httpClient = new HttpClient();

        var requestBody = new
        {
            idToken = idToken
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{apiBaseUrl}{TokenExchangePath}", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Token exchange failed: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent) ?? throw new InvalidDataException("Could not deserialize token response");

        return tokenResponse.AccessToken;
    }

    private static async Task ListAllUsers(string endpoint, string jwt)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        var response = await httpClient.GetAsync($"{endpoint}/users");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API call failed: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"API Response: {content}");
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
