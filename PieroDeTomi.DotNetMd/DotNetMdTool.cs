using Microsoft.Extensions.DependencyInjection;
using PieroDeTomi.DotNetMd.Contracts.Config;
using PieroDeTomi.DotNetMd.Contracts.Docs;
using PieroDeTomi.DotNetMd.Services.Extensions;

namespace PieroDeTomi.DotNetMd
{
    public class DotNetMdTool(string configFilePath, IServiceProvider serviceProvider)
    {
        private readonly DocGenerationConfig _configuration = serviceProvider.GetRequiredService<DocGenerationConfig>();

        private readonly IAssemblyDocParser _parser = serviceProvider.GetService<IAssemblyDocParser>();

        public void Run()
        {
            if (_configuration.Assemblies.Count == 0)
                throw new ApplicationException("No assemblies have been specified in the configuration file.");

            if (!Directory.Exists(_configuration.OutputPath))
                Directory.CreateDirectory(_configuration.OutputPath);

            var types = GetTypes(_configuration);

            var docGenerator = new DocGenerator(serviceProvider);
            docGenerator.GenerateDocs(basePath: GetConfigBasePath(), types);
        }

        private List<TypeModel> GetTypes(DocGenerationConfig configuration)
        {
            List<TypeModel> types = [];

            var baseConfigPath = GetConfigBasePath();

            configuration.Assemblies.ForEach(assemblyFilePath =>
            {
                var absoluteAssemblyFilePath = assemblyFilePath.MakeAbsolute(baseConfigPath);
                types.AddRange(_parser.ParseTypes(absoluteAssemblyFilePath));
            });

            return types;
        }

        private string GetConfigBasePath()
        {
            var absoluteConfigFilePath = Path.IsPathFullyQualified(configFilePath) ? configFilePath : Path.GetFullPath(configFilePath);
            return Path.GetDirectoryName(absoluteConfigFilePath);
        }
    }
}
