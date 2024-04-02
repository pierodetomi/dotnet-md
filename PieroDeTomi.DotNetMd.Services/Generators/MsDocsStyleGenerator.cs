using PieroDeTomi.DotNetMd.Contracts;
using PieroDeTomi.DotNetMd.Contracts.Config;
using PieroDeTomi.DotNetMd.Contracts.Docs;

namespace PieroDeTomi.DotNetMd.Services.Generators
{
    internal class MsDocsStyleGenerator : MarkdownGeneratorBase, IMarkdownDocsGenerator
    {
        private static readonly string _separator = $"{Environment.NewLine}{Environment.NewLine}";

        public MsDocsStyleGenerator(DocGenerationConfig configuration) : base(configuration) { }

        public string BuildMarkdown(TypeModel type, List<TypeModel> allTypes)
        {
            List<string> docParts = [];

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
    }
}
