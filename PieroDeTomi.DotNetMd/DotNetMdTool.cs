using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PieroDeTomi.DotNetMd.Contracts.Config;
using PieroDeTomi.DotNetMd.Contracts.Docs;

namespace PieroDeTomi.DotNetMd
{
    public class DotNetMdTool(string configFilePath, ILogger logger)
    {
        private readonly AssemblyXmlDocParser _parser = new AssemblyXmlDocParser(logger);

        public void Run()
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException("Configuration file not found", configFilePath);

            var configuration = JsonConvert.DeserializeObject<DocGenerationConfig>(File.ReadAllText(configFilePath));

            if (configuration.Assemblies.Count == 0)
                throw new ApplicationException("No assemblies have been specified in the configuration file.");

            if (!Directory.Exists(configuration.OutputPath))
                Directory.CreateDirectory(configuration.OutputPath);

            var types = GetTypes(configuration);
            
            var docGenerator = new DocGenerator(configuration, logger);
            docGenerator.GenerateDocs(types);
        }

        private List<TypeModel> GetTypes(DocGenerationConfig configuration)
        {
            List<TypeModel> types = [];

            configuration.Assemblies.ForEach(assemblyFilePath =>
            {
                _parser.LoadAssembly(assemblyFilePath);
                types.AddRange(_parser.GetTypes());
            });

            return types;
        }
    }
}
