﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Models.Theming;
using PieroDeTomi.DotNetMd.Contracts.Services;
using PieroDeTomi.DotNetMd.Contracts.Services.Context;
using PieroDeTomi.DotNetMd.Contracts.Services.Emitters;
using PieroDeTomi.DotNetMd.Contracts.Services.Generators;
using PieroDeTomi.DotNetMd.Contracts.Services.Parsers;
using PieroDeTomi.DotNetMd.Services.Context;
using PieroDeTomi.DotNetMd.Services.Emitters;
using PieroDeTomi.DotNetMd.Services.Generators.Default;
using PieroDeTomi.DotNetMd.Services.Generators.Microsoft;
using PieroDeTomi.DotNetMd.Services.Logging;
using PieroDeTomi.DotNetMd.Services.Parsers;

namespace PieroDeTomi.DotNetMd.Services
{
    public static class ServicesModule
    {
        public static void RegisterServices(this IServiceCollection services, string outputStyle)
        {
            services.AddScoped<ILogger, ConsoleLogger>();
            services.AddScoped<IAssemblyDocParser, AssemblyXmlDocParser>();
            services.AddScoped<IDocsEmitter, FileSystemDocsEmitter>();
            services.AddScoped<IEntryPoint, DotNetMdTool>();
            
            services.AddSingleton<IDocsGenerationContext, DocsGenerationContext>();
            
            switch (outputStyle)
            {
                case OutputStyles.Default:
                    services.AddScoped<IMarkdownDocsGenerator, DefaultGenerator>();
                    break;

                case OutputStyles.Microsoft:
                    services.AddScoped<IMarkdownDocsGenerator, MsDocsGenerator>();
                    break;

                default:
                    throw new ArgumentException($"Output style '{outputStyle}' is not supported");
            }
        }
    }
}
