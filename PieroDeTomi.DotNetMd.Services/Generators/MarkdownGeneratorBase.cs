using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using PieroDeTomi.DotNetMd.Contracts.Services.Context;
using PieroDeTomi.DotNetMd.Services.Generators.Default;
using System.Text.RegularExpressions;

namespace PieroDeTomi.DotNetMd.Services.Generators
{
    internal class MarkdownGeneratorBase
    {
        private readonly Regex _paramrefRegEx = new(@"<paramref\sname\=\""(?<name>[^\""]+)\""\s?\/>", RegexOptions.Compiled | RegexOptions.Multiline);

        private readonly Regex _inlineCodeRegEx = new(@"<c>(?<code>.*?)<\/c>", RegexOptions.Compiled | RegexOptions.Multiline);

        private readonly Regex _paragraphRegEx = new(@"<para>(?<content>.*?)<\/para>", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly Regex _seeRegEx = new(@"<see cref=""(?<reference>[^""]+)""\s?\/>", RegexOptions.Compiled | RegexOptions.Multiline);

        protected DocGenerationRuntimeConfig Configuration { get; private set; }

        protected IDocsGenerationContext Context { get; private set; }

        public MarkdownGeneratorBase(DocGenerationRuntimeConfig configuration, IDocsGenerationContext context)
        {
            Configuration = configuration;
            Context = context;
        }

        public string GetSanitizedFileName(string name)
        {
            return name.ToLower()
                .Replace(" ", string.Empty)
                .Replace(".", "-")
                .Replace("<", "__")
                .Replace(",", "__")
                .Replace(">", "__");
        }

        public void WriteDocusaurusCategoryFile(string folderPath, string label, int position, string description = null)
        {
            var filePath = Path.Combine(folderPath, "_category_.json");

            var content = DefaultTemplatesProvider.Current.DocusaurusCategory
                .Replace(TemplateTokens.LABEL, label)
                .Replace(TemplateTokens.POSITION, position.ToString())
                .Replace(TemplateTokens.DESCRIPTION, description ?? label);

            File.WriteAllText(filePath, content);
        }

        protected string TryGetDocLink(INamedObjectBaseModel namedObject, string currentNamespace)
        {
            if (namedObject is null)
                return null;

            return namedObject.ObjectCategory switch
            {
                ObjectCategory.Type => TryGetTypeDocLink(namedObject, currentNamespace),
                ObjectCategory.Method => TryGetMethodOrPropertyDocLink(namedObject, currentNamespace),
                ObjectCategory.Property => TryGetMethodOrPropertyDocLink(namedObject, currentNamespace),
                _ => null,
            };
        }

        protected string GetSafeMarkdownText(string sourceText, string currentNamespace, bool isTableCell = false)
        {
            // Do proper transformations and escaping for safe markdown output
            if (sourceText is null)
                return null;

            sourceText = ReplaceRegex(
                _paramrefRegEx,
                sourceText,
                "name",
                name => $"`{name}`");

            sourceText = ReplaceRegex(
                _paragraphRegEx,
                sourceText,
                "content",
                content => content);

            sourceText = ReplaceRegex(
                _seeRegEx,
                sourceText,
                "reference",
                reference =>
                {
                    var namedObject = Context.FindObjectByIdentifier(reference);
                    return namedObject is not null
                        ? $"[`{namedObject.Name}`]({TryGetDocLink(namedObject, currentNamespace)})"
                        : $"`{reference}`";
                });

            sourceText = ReplaceRegex(
                _inlineCodeRegEx,
                sourceText,
                "code",
                code => $"`{code.Replace("`", string.Empty)}`");

            if (isTableCell)
            {
                // Cell text cannot start with or contain a newline. Trim and replace newlines with <br />
                sourceText = sourceText
                    .Trim()
                    .Replace(Environment.NewLine, "<br />")
                    .Replace("|", "\\|");
            }

            return sourceText.Trim();
        }

        protected static string ReplaceRegex(Regex regex, string text, string groupName, Func<string, string> replacementGenerator)
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

        private string TryGetTypeDocLink(INamedObjectBaseModel namedObject, string currentNamespace)
        {
            var basePath = "./";

            if (Configuration.ShouldCreateNamespaceFolders && namedObject.Namespace != currentNamespace)
                basePath = $"../{GetNamespaceForRouting(namedObject.Namespace)}/";

            return $"{basePath}{GetSanitizedFileName(namedObject.Name)}";
        }

        private string TryGetMethodOrPropertyDocLink(INamedObjectBaseModel namedObject, string currentNamespace)
        {
            var basePath = "./";

            if (Configuration.ShouldCreateNamespaceFolders && namedObject.Namespace != currentNamespace)
                basePath = $"../{GetNamespaceForRouting(namedObject.Owner.Namespace)}/";

            return $"{basePath}{GetSanitizedFileName(namedObject.Owner.Name)}#{namedObject.Name.ToLower().Replace(" ", "-")}";
        }

        private string GetNamespaceForRouting(string namespaceName)
        {
            var alias = Configuration.DocusaurusOptions?.PartialNamespaceAliasForLabels ?? string.Empty;

            if (alias?.Length > 0 && namespaceName.StartsWith($"{alias}."))
                namespaceName = namespaceName.Replace($"{alias}.", string.Empty);

            return GetSanitizedFileName(namespaceName);
        }
    }
}
