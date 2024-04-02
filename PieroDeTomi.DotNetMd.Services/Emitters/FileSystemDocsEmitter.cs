using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using PieroDeTomi.DotNetMd.Contracts.Services.Emitters;
using PieroDeTomi.DotNetMd.Contracts.Services.Generators;
using PieroDeTomi.DotNetMd.Services.Extensions;

namespace PieroDeTomi.DotNetMd.Services.Emitters
{
    internal class FileSystemDocsEmitter : IDocsEmitter
    {
        private readonly DocGenerationRuntimeConfig _configuration;

        private readonly IMarkdownDocsGenerator _generator;
        
        private readonly ILogger _logger;

        public FileSystemDocsEmitter(DocGenerationRuntimeConfig configuration, IMarkdownDocsGenerator generator, ILogger logger)
        {
            _configuration = configuration;
            _generator = generator;
            _logger = logger;
        }

        public void Emit(List<TypeModel> types)
        {
            var outputFolder = _configuration.OutputPath.MakeAbsolute(_configuration.BasePath);

            CleanupOutputPath(outputFolder);

            var currentNamespace = string.Empty;
            var namespaceCount = 0;
            var currentFileIndex = 0;

            // Sort all types by namespace and (then by) name
            types
                .OrderBy(type => type.Namespace)
                .ThenBy(type => type.Name)
                .ToList()
                .ForEach(type =>
                {
                    _logger.LogInformation($"Generating documentation for {type.Name}");

                    currentFileIndex++;

                    var isNewNamespace = type.Namespace != currentNamespace;
                    
                    if (isNewNamespace)
                    {
                        currentNamespace = type.Namespace;
                        namespaceCount++;

                        currentFileIndex = 1;
                    }

                    var targetFolder = outputFolder;

                    if (_configuration.ShouldCreateNamespaceFolders)
                    {
                        var namespaceFolderName = _generator.GetSanitizedFileName(type.Namespace);
                        var namespaceFolderPath = Path.Combine(targetFolder, namespaceFolderName);

                        if (!Directory.Exists(namespaceFolderPath))
                            Directory.CreateDirectory(namespaceFolderPath);

                        if (isNewNamespace && _configuration.IsDocusaurusProject)
                            _generator.WriteDocusaurusCategoryFile(namespaceFolderPath, type.Namespace, namespaceCount, $"{type.Namespace} namespace documentation");

                        targetFolder = namespaceFolderPath;
                    }

                    var markdown = _generator.BuildMarkdown(type, types);
                    var targetFileName = $"{_generator.GetSanitizedFileName(type.Name)}.md";

                    File.WriteAllText(Path.Combine(targetFolder, targetFileName), markdown);
                });
        }

        private static void CleanupOutputPath(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                return;
            }

            Directory
                .GetDirectories(folder)
                .ToList()
                .ForEach(directory => Directory.Delete(directory, recursive: true));

            Directory.GetFiles(folder)
                .ToList()
                .ForEach(File.Delete);
        }
    }
}
