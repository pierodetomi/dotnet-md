using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Config;
using PieroDeTomi.DotNetMd.Contracts.Docs;
using PieroDeTomi.DotNetMd.Extensions;
using System.Text.RegularExpressions;

namespace PieroDeTomi.DotNetMd
{
    public class DocGenerator(DocGenerationConfig configuration, ILogger logger)
    {
        private static readonly string _separator = $"{Environment.NewLine}{Environment.NewLine}";

        public void GenerateDocs(string basePath, List<TypeModel> types)
        {
            var outputFolder = configuration.OutputPath.MakeAbsolute(basePath);

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
                    logger.LogInformation($"Generating documentation for {type.Name}");

                    currentFileIndex++;

                    var isNewNamespace = type.Namespace != currentNamespace;
                    
                    if (isNewNamespace)
                    {
                        currentNamespace = type.Namespace;
                        namespaceCount++;

                        currentFileIndex = 1;
                    }

                    var targetFolder = outputFolder;

                    if (configuration.ShouldCreateNamespaceFolders)
                    {
                        var namespaceFolderName = GetSanitizedName(type.Namespace);
                        var namespaceFolderPath = Path.Combine(targetFolder, namespaceFolderName);

                        if (!Directory.Exists(namespaceFolderPath))
                            Directory.CreateDirectory(namespaceFolderPath);

                        if (isNewNamespace && configuration.IsDocusaurusProject)
                            File.WriteAllText(Path.Combine(namespaceFolderPath, "_category_.json"), DocTemplates.Current.DocusaurusCategory
                                .Replace("{{LABEL}}", type.Namespace)
                                .Replace("{{POSITION}}", namespaceCount.ToString())
                                .Replace("{{DESCRIPTION}}", $"{type.Namespace} namespace documentation"));

                        targetFolder = namespaceFolderPath;
                    }

                    var markdown = BuildMarkdown(type, currentFileIndex, types);
                    var targetFileName = $"{GetSanitizedName(type.Name)}.md";

                    File.WriteAllText(Path.Combine(targetFolder, targetFileName), markdown);
                });
        }

        private string BuildMarkdown(TypeModel type, int fileIndex, List<TypeModel> allTypes)
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
                var properties = string.Join(Environment.NewLine, type.Properties.Select(p =>
                {
                    var propertyTypeReference = allTypes.FirstOrDefault(t => t.Name == p.Type.Name);
                    var propertyTypeLink = TryGetDocLink(propertyTypeReference, currentNamespace: type.Namespace);
                    var typeNameColumn = propertyTypeLink is not null ? $"[`{p.Type.Name}`]({propertyTypeLink})" : $"`{p.Type.Name}`";

                    return $"| `{p.Name}` | {typeNameColumn} | {GetSafeMarkdownText(p.Type.Summary, isTableCell: true)} |";
                }));
                properties = DocTemplates.Current.Properties.Replace("{{PROPERTIES}}", properties);
                docParts.Add(properties);
            }

            if (type.Methods.Count > 0)
            {
                var methods = string.Join(Environment.NewLine, type.Methods.Select(m =>
                {
                    return $"| `{m.GetSignature()}` | {GetSafeMarkdownText(m.Summary, isTableCell: true)} | {GetSafeMarkdownText(m.Returns, isTableCell: true)} |";
                }));

                methods = DocTemplates.Current.Methods.Replace("{{METHODS}}", methods);
                docParts.Add(methods);
            }

            return string.Join(_separator, docParts);
        }

        private string TryGetDocLink(TypeModel type, string currentNamespace)
        {
            if (type is null)
                return null;

            var basePath = "./";
            
            if (configuration.ShouldCreateNamespaceFolders && type.Namespace != currentNamespace)
                basePath = $"../{type.Namespace}/";

            return $"{basePath}{GetSanitizedName(type.Name)}";
        }

        private static string GetSafeMarkdownText(string sourceText, bool isTableCell = false)
        {
            // Do proper transformations and escaping for safe markdown output
            if (sourceText is null)
                return null;

            sourceText = ReplaceRegex(
                new Regex(@"<paramref\sname\=\""(?<name>[^\""]+)\""\s?\/>", RegexOptions.Multiline),
                sourceText,
                "name",
                name => $"`{name}`");

            sourceText = ReplaceRegex(
                new Regex(@"<c>(?<code>[^<]+)<\/c>", RegexOptions.Multiline),
                sourceText,
                "code",
                code => $"`{code}`");

            sourceText = ReplaceRegex(
                new Regex(@"<para>(?<content>.*?)<\/para>", RegexOptions.Singleline),
                sourceText,
                "content",
                content => $"{Environment.NewLine}{content}{Environment.NewLine}");

            sourceText = sourceText.Replace("|", @"\|");

            if (isTableCell)
            {
                // Cell text cannot start with or contain a newline. Trim and replace newlines with <br />
                sourceText = sourceText
                    .Trim()
                    .Replace(Environment.NewLine, "<br />");

                // TODO: Escape markdown table cell pipes
            }

            return sourceText;
        }

        private static string ReplaceRegex(Regex regex, string text, string groupName, Func<string, string> replacementGenerator)
        {
            regex
                .Matches(text)
                .OrderByDescending(m => m.Index)
                .ToList()
                .ForEach(match =>
                {
                    text = text
                        .Remove(match.Index, match.Length)
                        .Insert(match.Index, replacementGenerator(match.Groups[groupName].Value));
                });

            return text;
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
