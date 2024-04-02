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
        var configurationOption = new Option<FileInfo>(
            name: "--config",
            description: "The configuration file to use for parsing & generation");

        var rootCommand = new RootCommand("DotNetMd is a simple tool for generating Markdown docs from .NET assemblies");
        rootCommand.AddOption(configurationOption);

        rootCommand.SetHandler(configurationFile =>
        {
            BuildServiceProvider(configurationFile)
                .GetRequiredService<DotNetMdTool>()
                .Run();
        },
        configurationOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static DocGenerationRuntimeConfig GetRuntimeConfiguration(string configFilePath, DocGenerationConfig configuration)
    {
        var absoluteConfigFilePath = Path.IsPathFullyQualified(configFilePath) ? configFilePath : Path.GetFullPath(configFilePath);

        return new DocGenerationRuntimeConfig
        {
            Assemblies = configuration.Assemblies,
            OutputPath = configuration.OutputPath,
            IsDocusaurusProject = configuration.IsDocusaurusProject,
            OutputStyle = configuration.OutputStyle,
            ShouldCreateNamespaceFolders = configuration.ShouldCreateNamespaceFolders,
            BasePath = Path.GetDirectoryName(absoluteConfigFilePath)
        };
    }

    private static IServiceProvider BuildServiceProvider(FileInfo configurationFile)
    {
#if DEBUG
        configurationFile ??= new FileInfo(@"..\..\..\..\_assets\dotnetmd.json");
#else
        configurationFile.ThrowIfNull("Configuration file is required");
#endif

        if (!File.Exists(configurationFile.FullName))
            throw new FileNotFoundException("Configuration file not found", configurationFile.FullName);

        var configuration = JsonConvert.DeserializeObject<DocGenerationConfig>(File.ReadAllText(configurationFile.FullName));
        var runtimeConfiguration = GetRuntimeConfiguration(configurationFile.FullName, configuration);

        // Setup DI container
        var services = new ServiceCollection();
        services.AddScoped(p => runtimeConfiguration);
        services.AddScoped<DotNetMdTool>();
        
        DIModule.RegisterServices(services);

        return services.BuildServiceProvider();
    }
}