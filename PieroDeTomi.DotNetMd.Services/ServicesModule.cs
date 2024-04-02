using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Services;
using PieroDeTomi.DotNetMd.Contracts.Services.Emitters;
using PieroDeTomi.DotNetMd.Contracts.Services.Generators;
using PieroDeTomi.DotNetMd.Contracts.Services.Parsers;
using PieroDeTomi.DotNetMd.Services.Emitters;
using PieroDeTomi.DotNetMd.Services.Generators;
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
            
            switch (outputStyle)
            {
                case OutputStyles.Default:
                    services.AddScoped<IMarkdownDocsGenerator, MsDocsStyleGenerator>();
                    break;

                case OutputStyles.Microsoft:
                    services.AddScoped<IMarkdownDocsGenerator, MsDocsStyleGenerator>();
                    break;

                default:
                    throw new ArgumentException($"Output style '{outputStyle}' is not supported");
            }
        }
    }
}
