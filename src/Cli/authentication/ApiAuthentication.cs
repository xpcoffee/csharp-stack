using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cli.Authentication.Api;

public class ApiAuthentication
{
    private const string TokenExchangePath = "/api/Auth/google";
    private const string JwtFilePath = ".csharp-stack-cli-jwt.txt";

    public static async Task<string> ExchangeIdTokenForJwtAsync(string apiBaseUrl, string idToken)
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
        Console.WriteLine($"response: {responseContent}");
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent) ?? throw new InvalidDataException("Could not deserialize token response");

        return tokenResponse.token;
    }


    public static void SaveJwtToFile(string jwtToken)
    {
        try
        {
            File.WriteAllText(JwtFilePath, jwtToken);
            Console.WriteLine($"JWT token written to {JwtFilePath}");
        }
        catch (Exception ex)
        {
            throw new Exception("Error writing jwt token to file", ex);
        }
    }

    public static string ReadJwtFromFile()
    {
        try
        {
            return File.ReadAllText(JwtFilePath);
        }
        catch (Exception ex)
        {
            throw new Exception("Error reading jwt token from file", ex);
        }
    }
}

public class TokenResponse
{
    public string token { get; set; } = string.Empty;
}
