using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using PieroDeTomi.DotNetMd.Contracts.Services.Generators;

namespace PieroDeTomi.DotNetMd.Services.Generators.Microsoft
{
    internal class MsDocsGenerator : MarkdownGeneratorBase, IMarkdownDocsGenerator
    {
        private static readonly string _separator = $"{Environment.NewLine}{Environment.NewLine}";

        public MsDocsGenerator(DocGenerationRuntimeConfig configuration) : base(configuration) { }

        public string BuildMarkdown(TypeModel type, List<TypeModel> allTypes)
        {
            List<string> docParts = [];

            docParts.Add(MsDocsTemplatesProvider.Current.Header
                .Replace(TemplateTokens.NAME, type.Name)
                .Replace(TemplateTokens.TYPE, type.ObjectType)
                .Replace(TemplateTokens.NAMESPACE, type.Namespace)
                .Replace(TemplateTokens.ASSEMBLY, type.Assembly)
                .Replace(TemplateTokens.DECLARATION, type.Declaration)
                .Replace(TemplateTokens.SUMMARY, type.Summary));

            if (type.InheritanceChain.Count > 0)
            {
                docParts.Add("## Inheritance");
                docParts.Add($"`{type.Name}` &rarr; {string.Join(" &rarr; ", type.InheritanceChain.Select(t => $"`{t}`"))}");
            }

            if (type.TypeParameters.Count > 0)
            {
                var typeParams = string.Join(_separator, type.TypeParameters.Select(tp =>
                    MsDocsTemplatesProvider.Current.TypeParam
                        .Replace(TemplateTokens.NAME, tp.Name)
                        .Replace(TemplateTokens.DESCRIPTION, tp.Description)));

                typeParams = MsDocsTemplatesProvider.Current.TypeParams.Replace(TemplateTokens.PARAMS, typeParams);

                docParts.Add(typeParams);
            }

            if (type.Remarks is not null)
                docParts.Add(MsDocsTemplatesProvider.Current.Remarks.Replace(TemplateTokens.REMARKS, type.Remarks));

            if (type.Properties.Count > 0)
            {
                var properties = string.Join(Environment.NewLine, type.Properties.Select(p =>
                {
                    var propertyTypeReference = allTypes.FirstOrDefault(t => t.Name == p.Type.Name);
                    var propertyTypeLink = TryGetDocLink(propertyTypeReference, currentNamespace: type.Namespace);
                    var typeNameColumn = propertyTypeLink is not null ? $"[`{p.Type.Name}`]({propertyTypeLink})" : $"`{p.Type.Name}`";

                    return $"| `{p.Name}` | {typeNameColumn} | {GetSafeMarkdownText(p.Type.Summary, isTableCell: true)} |";
                }));
                properties = MsDocsTemplatesProvider.Current.Properties.Replace(TemplateTokens.PROPERTIES, properties);
                docParts.Add(properties);
            }

            if (type.Methods.Count > 0)
            {
                var methods = string.Join(Environment.NewLine, type.Methods.Select(m =>
                {
                    var parameters = string.Empty;
                    
                    if (m.HasParameters)
                    {
                        parameters = $"<br /><br />**Parameters**<br />";
                        parameters += string.Join("<br />", m.Parameters.Select(parameter => $"- **`{parameter.Type}` {parameter.Name}:** {GetSafeMarkdownText(parameter.Description)}"));
                    }
                    
                    return $"| `{m.GetSignature()}` | {GetSafeMarkdownText(m.Summary, isTableCell: true)}{parameters} | {GetSafeMarkdownText(m.Returns, isTableCell: true)} |";
                }));

                methods = MsDocsTemplatesProvider.Current.Methods.Replace(TemplateTokens.METHODS, methods);
                docParts.Add(methods);
            }

            return string.Join(_separator, docParts);
        }
    }
}
