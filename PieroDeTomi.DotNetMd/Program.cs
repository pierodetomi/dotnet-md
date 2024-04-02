using PieroDeTomi.DotNetMd;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configurationOption = new Option<FileInfo>(
            name: "--config",
            description: "The configuration file to use for parsing & generation");

        var rootCommand = new RootCommand("DotNetMd is a simple tool for generating Markdown docs from .NET assemblies");
        rootCommand.AddOption(configurationOption);

        rootCommand.SetHandler(configurationFile =>
        {
            configurationFile ??= new FileInfo(@"..\..\..\..\_assets\dotnetmd.json");

            var logger = new ConsoleLogger();
            var tool = new DotNetMdTool(configurationFile.FullName, logger);

            tool.Run();

            Console.WriteLine($"{Environment.NewLine}Generation completed. Press any key to exit ...");
            Console.ReadKey();
        },
        configurationOption);

        return await rootCommand.InvokeAsync(args);
    }
}