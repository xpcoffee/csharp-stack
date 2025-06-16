using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cli.Api.Helpers;
using Cli.Authentication.Api;
using CommandLine;
using Microsoft.Extensions.Configuration;

namespace Cli.Commands.Users;

public class UsersCommand
{
    public static async Task<int> Handle(UserOptions options, IConfigurationRoot configuration)
    {
        try
        {
            var baseUrl = ApiHelpers.GetBaseUrl(options.Endpoint);
            var jwt = ApiAuthentication.ReadJwtFromFile();
            await ListAllUsers(baseUrl, jwt);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Issue interacting with users: {ex}");
            return 1;
        }

    }

    private static async Task ListAllUsers(string endpoint, string jwt)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        var response = await httpClient.GetAsync($"{endpoint}/api/users");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API call failed: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }
}

[Verb("list-users", HelpText = "List all users")]
public class UserOptions : BaseCommand { }
