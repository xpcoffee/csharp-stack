using System;

namespace Cli.Api.Helpers;

public static class ApiHelpers
{
    public static string GetBaseUrl(string endpoint)
    {
        var builder = new UriBuilder(endpoint);
        return $"{builder.Scheme}://{builder.Host}:{builder.Port}";
    }
}
