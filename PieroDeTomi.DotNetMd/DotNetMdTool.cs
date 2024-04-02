using PieroDeTomi.DotNetMd.Contracts.Config;
using PieroDeTomi.DotNetMd.Contracts.Docs;
using PieroDeTomi.DotNetMd.Services.Extensions;

namespace PieroDeTomi.DotNetMd
{
    public class DotNetMdTool(DocGenerationRuntimeConfig configuration, IAssemblyDocParser parser, IServiceProvider serviceProvider)
    {
        public void Run()
        {
            if (configuration.Assemblies.Count == 0)
                throw new ApplicationException("No assemblies have been specified in the configuration file.");

            if (!Directory.Exists(configuration.OutputPath))
                Directory.CreateDirectory(configuration.OutputPath);

            var types = GetTypes();

            var docGenerator = new DocGenerator(serviceProvider);
            docGenerator.GenerateDocs(types);
        }

        private List<TypeModel> GetTypes()
        {
            List<TypeModel> types = [];

            configuration.Assemblies.ForEach(assemblyFilePath =>
            {
                var absoluteAssemblyFilePath = assemblyFilePath.MakeAbsolute(configuration.BasePath);
                types.AddRange(parser.ParseTypes(absoluteAssemblyFilePath));
            });

            return types;
        }
    }
}
