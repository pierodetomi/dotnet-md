using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts;
using PieroDeTomi.DotNetMd.Contracts.Docs;
using PieroDeTomi.DotNetMd.Services.Generators;
using PieroDeTomi.DotNetMd.Services.Logging;
using PieroDeTomi.DotNetMd.Services.Parsers;

namespace PieroDeTomi.DotNetMd.Services
{
    public class DIModule
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ILogger, ConsoleLogger>();
            services.AddScoped<IAssemblyDocParser, AssemblyXmlDocParser>();
            
            services.AddKeyedScoped<IMarkdownDocsGenerator, MsDocsStyleGenerator>(OutputStyles.Default);
            services.AddKeyedScoped<IMarkdownDocsGenerator, MsDocsStyleGenerator>(OutputStyles.Microsoft);
        }
    }
}
