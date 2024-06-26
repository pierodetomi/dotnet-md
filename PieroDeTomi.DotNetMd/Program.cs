﻿using Microsoft.Extensions.DependencyInjection;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using PieroDeTomi.DotNetMd.Contracts.Services;
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
                .GetRequiredService<IEntryPoint>()
                .Run();
        },
        configurationOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static DocGenerationRuntimeConfig GetRuntimeConfiguration(FileInfo configurationFile)
    {
        if (!File.Exists(configurationFile.FullName))
            throw new FileNotFoundException("Configuration file not found", configurationFile.FullName);

        TinyMapper.Bind<DocGenerationConfig, DocGenerationRuntimeConfig>();

        var configFilePath = configurationFile.FullName;
        var configuration = JsonConvert.DeserializeObject<DocGenerationConfig>(File.ReadAllText(configFilePath));
        
        var runtimeConfiguration = TinyMapper.Map<DocGenerationRuntimeConfig>(configuration);
        
        var absoluteConfigFilePath = Path.IsPathFullyQualified(configFilePath) ? configFilePath : Path.GetFullPath(configFilePath);
        runtimeConfiguration.BasePath = Path.GetDirectoryName(absoluteConfigFilePath);

        return runtimeConfiguration;
    }

    private static IServiceProvider BuildServiceProvider(FileInfo configurationFile)
    {
#if DEBUG
        configurationFile ??= new FileInfo(@"..\..\..\..\_demo\dotnetmd.json");
#else
        ArgumentNullException.ThrowIfNull(configurationFile);
#endif

        var runtimeConfiguration = GetRuntimeConfiguration(configurationFile);

        var services = new ServiceCollection();

        services.AddScoped(p => runtimeConfiguration);
        services.RegisterServices(runtimeConfiguration.OutputStyle);

        return services.BuildServiceProvider();
    }
}