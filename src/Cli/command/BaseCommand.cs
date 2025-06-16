using CommandLine;

public class BaseCommand
{
    [Option('e', "endpoint", Required = false, HelpText = "The API endpoint e.g. http://localhost:5001")]
    public string Endpoint { get; set; } = string.Empty;
}

