using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PieroDeTomi.DotNetMd;
using PieroDeTomi.DotNetMd.Contracts.Config;
using PieroDeTomi.DotNetMd.Services;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Setup DI container
        var services = new ServiceCollection();
        DIModule.RegisterServices(services);

        var configurationOption = new Option<FileInfo>(
            name: "--config",
            description: "The configuration file to use for parsing & generation");

        var rootCommand = new RootCommand("DotNetMd is a simple tool for generating Markdown docs from .NET assemblies");
        rootCommand.AddOption(configurationOption);

        rootCommand.SetHandler(configurationFile =>
        {

#if DEBUG
            configurationFile ??= new FileInfo(@"..\..\..\..\_assets\dotnetmd.json");
#else
            configurationFile.ThrowIfNull("Configuration file is required");
#endif

            if (!File.Exists(configurationFile.FullName))
                throw new FileNotFoundException("Configuration file not found", configurationFile.FullName);

            var configuration = JsonConvert.DeserializeObject<DocGenerationConfig>(File.ReadAllText(configurationFile.FullName));
            services.AddScoped(p => configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            
            var tool = new DotNetMdTool(configurationFile.FullName, serviceProvider);

            tool.Run();
        },
        configurationOption);

        return await rootCommand.InvokeAsync(args);
    }
}