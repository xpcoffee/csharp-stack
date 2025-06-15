using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class GoogleDeviceFlowAuth
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;

    // Google OAuth 2.0 endpoints
    private const string DeviceAuthorizationEndpoint = "https://oauth2.googleapis.com/device/code";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    public GoogleDeviceFlowAuth(string clientId, string clientSecret)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _httpClient = new HttpClient();
    }

    public async Task<string> AuthenticateAsync(string[] scopes)
    {
        try
        {
            // Step 1: Request device and user codes
            var deviceResponse = await RequestDeviceCodeAsync(scopes);

            // Step 2: Display user code and verification URL to user
            Console.WriteLine($"Please go to: {deviceResponse.VerificationUrl}");
            Console.WriteLine($"And enter the code: {deviceResponse.UserCode}");
            Console.WriteLine();
            Console.WriteLine("Waiting for authentication...");

            // Step 3: Poll for access token
            var accessToken = await PollForIdTokenAsync(deviceResponse);

            Console.WriteLine("Authentication successful!");
            return accessToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
            throw;
        }
    }

    private async Task<DeviceCodeResponse> RequestDeviceCodeAsync(string[] scopes)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["scope"] = string.Join(" ", scopes)
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(DeviceAuthorizationEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to request device code: {response.StatusCode} - {errorContent}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var deviceResponse = JsonSerializer.Deserialize<DeviceCodeResponse>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? throw new Exception($"Empty result when deserializing device code response: {jsonResponse}");

        return deviceResponse;
    }

    private async Task<string> PollForIdTokenAsync(DeviceCodeResponse deviceResponse)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret, // TODO: why is client secret needed?
            ["device_code"] = deviceResponse.DeviceCode,
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
        };

        var content = new FormUrlEncodedContent(parameters);
        var interval = deviceResponse.Interval;
        var expiresAt = DateTime.UtcNow.AddSeconds(deviceResponse.ExpiresIn);

        while (DateTime.UtcNow < expiresAt)
        {
            await Task.Delay(interval * 1000); // Convert to milliseconds

            var response = await _httpClient.PostAsync(TokenEndpoint, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }) ?? throw new Exception("Token response was null");

                return tokenResponse.IdToken;
            }

            // Parse error response
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            }) ?? throw new Exception($"Empty result deserializing error response {jsonResponse}");

            switch (errorResponse.Error)
            {
                case "authorization_pending":
                    // Continue polling
                    Console.Write(".");
                    break;
                case "slow_down":
                    // Increase polling interval
                    interval += 5;
                    Console.WriteLine(" (Slowing down polling...)");
                    break;
                case "expired_token":
                    throw new Exception("The device code has expired. Please restart the authentication process.");
                case "access_denied":
                    throw new Exception("The user denied the authentication request.");
                default:
                    throw new Exception($"Authentication error: {errorResponse.Error} - {errorResponse.ErrorDescription}");
            }
        }

        throw new Exception("Authentication timed out. Please restart the process.");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Data models for JSON responses
public class DeviceCodeResponse
{
    public required string DeviceCode { get; set; }
    public required string UserCode { get; set; }
    public required string VerificationUrl { get; set; }
    public string? VerificationUrlComplete { get; set; }
    public required int ExpiresIn { get; set; }
    public required int Interval { get; set; }
}

public class TokenResponse
{
    public required string AccessToken { get; set; }
    public required string IdToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string TokenType { get; set; }
    public required int ExpiresIn { get; set; }
    public required string Scope { get; set; }
}

public class ErrorResponse
{
    public required string Error { get; set; }
    public required string ErrorDescription { get; set; }
}

