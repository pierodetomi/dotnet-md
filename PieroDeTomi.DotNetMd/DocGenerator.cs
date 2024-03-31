using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Config;
using PieroDeTomi.DotNetMd.Contracts.Docs;

namespace PieroDeTomi.DotNetMd
{
    public class DocGenerator(DocGenerationConfig configuration, ILogger logger)
    {
        private static readonly string _separator = $"{Environment.NewLine}{Environment.NewLine}";

        public void GenerateDocs(List<TypeModel> types)
        {
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
                    logger.LogInformation($"Generating documentation for {type.Name}");

                    currentFileIndex++;

                    var isNewNamespace = type.Namespace != currentNamespace;
                    
                    if (isNewNamespace)
                    {
                        currentNamespace = type.Namespace;
                        namespaceCount++;

                        currentFileIndex = 1;
                    }

                    var targetFolder = configuration.OutputPath;

                    if (configuration.ShouldCreateNamespaceFolders)
                    {
                        var namespaceFolderName = GetSanitizedName(type.Namespace);
                        var namespaceFolderPath = Path.Combine(configuration.OutputPath, namespaceFolderName);

                        if (!Directory.Exists(namespaceFolderPath))
                        {
                            Directory.CreateDirectory(namespaceFolderPath);

                        }

                        if (isNewNamespace && configuration.IsDocusaurusProject)
                            File.WriteAllText(Path.Combine(namespaceFolderPath, "_category_.json"), DocTemplates.Current.DocusaurusCategory
                                .Replace("{{LABEL}}", type.Namespace)
                                .Replace("{{POSITION}}", namespaceCount.ToString())
                                .Replace("{{DESCRIPTION}}", $"{type.Namespace} namespace documentation"));

                        targetFolder = namespaceFolderPath;
                    }

                    var markdown = BuildMarkdown(type, currentFileIndex);
                    var targetFileName = $"{GetSanitizedName(type.Name)}.md";

                    File.WriteAllText(Path.Combine(targetFolder, targetFileName), markdown);
                });
        }

        private string BuildMarkdown(TypeModel type, int fileIndex)
        {
            List<string> docParts = [];

            if (configuration.IsDocusaurusProject)
            {
                docParts.Add(DocTemplates.Current.DocusaurusFrontMatter
                    .Replace("{{SIDEBAR_LABEL}}", type.Name)
                    .Replace("{{SIDEBAR_POSITION}}", fileIndex.ToString()));
            }

            docParts.Add(DocTemplates.Current.Header
                .Replace("{{NAME}}", type.Name)
                .Replace("{{TYPE}}", type.ObjectType)
                .Replace("{{NAMESPACE}}", type.Namespace)
                .Replace("{{ASSEMBLY}}", type.Assembly)
                .Replace("{{DECLARATION}}", type.Declaration)
                .Replace("{{SUMMARY}}", type.Summary));

            if (type.TypeParameters.Count > 0)
            {
                var typeParams = string.Join(_separator, type.TypeParameters.Select(tp =>
                    DocTemplates.Current.TypeParam
                        .Replace("{{NAME}}", tp.Name)
                        .Replace("{{DESCRIPTION}}", tp.Description)));

                typeParams = DocTemplates.Current.TypeParams.Replace("{{PARAMS}}", typeParams);

                docParts.Add(typeParams);
            }

            if (type.Remarks is not null)
                docParts.Add(DocTemplates.Current.Remarks.Replace("{{REMARKS}}", type.Remarks));

            if (type.Properties.Count > 0)
            {
                var properties = string.Join(Environment.NewLine, type.Properties.Select(p => $"| `{p.Name}` | `{p.Type.Name}` | {p.Type.Summary} |"));
                properties = DocTemplates.Current.Properties.Replace("{{PROPERTIES}}", properties);
                docParts.Add(properties);
            }

            if (type.Methods.Count > 0)
            {
                var methods = string.Join(Environment.NewLine, type.Methods.Select(m =>
                {
                    return $"| `{m.GetSignature()}` | {m.Summary} | {m.Returns} |";
                }));

                methods = DocTemplates.Current.Methods.Replace("{{METHODS}}", methods);
                docParts.Add(methods);
            }

            return string.Join(_separator, docParts);
        }

        private static string GetSanitizedName(string name)
        {
            return name.ToLower()
                .Replace(" ", string.Empty)
                .Replace(".", "-")
                .Replace("<", "__")
                .Replace(",", "__")
                .Replace(">", "__");
        }
    }
}
