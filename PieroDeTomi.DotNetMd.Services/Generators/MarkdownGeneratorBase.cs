using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using System.Text.RegularExpressions;

namespace PieroDeTomi.DotNetMd.Services.Generators
{
    internal class MarkdownGeneratorBase
    {
        protected DocGenerationRuntimeConfig Configuration { get; private set; }

        public MarkdownGeneratorBase(DocGenerationRuntimeConfig configuration)
        {
            Configuration = configuration;
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

            var content = DocTemplates.Current.DocusaurusCategory
                .Replace("{{LABEL}}", label)
                .Replace("{{POSITION}}", position.ToString())
                .Replace("{{DESCRIPTION}}", description ?? label);

            File.WriteAllText(filePath, content);
        }

        protected string TryGetDocLink(TypeModel type, string currentNamespace)
        {
            if (type is null)
                return null;

            var basePath = "./";

            if (Configuration.ShouldCreateNamespaceFolders && type.Namespace != currentNamespace)
                basePath = $"../{type.Namespace}/";

            return $"{basePath}{GetSanitizedFileName(type.Name)}";
        }

        protected string GetSafeMarkdownText(string sourceText, bool isTableCell = false)
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
                content => content);

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
    }
}
