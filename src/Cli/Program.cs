using System;
using System.IO;
using System.Threading.Tasks;
using Cli.Commands.Authentication;
using Cli.Commands.Users;
using CommandLine;
using Microsoft.Extensions.Configuration;

namespace Cli.Program;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            DotNetEnv.Env.Load();
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddEnvironmentVariables().Build();

            var result = await Parser.Default.ParseArguments<AuthOptions, UserOptions>(args)
                   .MapResult(
                       parsedFunc1: (AuthOptions opts) => AuthenticationCommand.Handle(opts, configuration),
                       (UserOptions opts) => UsersCommand.Handle(opts, configuration),
                       errs => Task.FromResult(1));

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

}
