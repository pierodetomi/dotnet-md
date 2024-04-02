using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using PieroDeTomi.DotNetMd.Contracts.Services;
using PieroDeTomi.DotNetMd.Contracts.Services.Emitters;
using PieroDeTomi.DotNetMd.Contracts.Services.Parsers;
using PieroDeTomi.DotNetMd.Services.Extensions;

namespace PieroDeTomi.DotNetMd.Services
{
    internal class DotNetMdTool : IEntryPoint
    {
        private readonly DocGenerationRuntimeConfig _configuration;
        
        private readonly IAssemblyDocParser _parser;
        
        private readonly IDocsEmitter _emitter;

        public DotNetMdTool(DocGenerationRuntimeConfig configuration, IAssemblyDocParser parser, IDocsEmitter emitter)
        {
            _configuration = configuration;
            _parser = parser;
            _emitter = emitter;
        }

        public void Run()
        {
            if (_configuration.Assemblies.Count == 0)
                throw new ApplicationException("No assemblies have been specified in the configuration file.");

            if (!Directory.Exists(_configuration.OutputPath))
                Directory.CreateDirectory(_configuration.OutputPath);

            var types = GetTypes();

            _emitter.Emit(types);
        }

        private List<TypeModel> GetTypes()
        {
            List<TypeModel> types = [];

            _configuration.Assemblies.ForEach(assemblyFilePath =>
            {
                var absoluteAssemblyFilePath = assemblyFilePath.MakeAbsolute(_configuration.BasePath);
                types.AddRange(_parser.ParseTypes(absoluteAssemblyFilePath));
            });

            return types;
        }
    }
}
