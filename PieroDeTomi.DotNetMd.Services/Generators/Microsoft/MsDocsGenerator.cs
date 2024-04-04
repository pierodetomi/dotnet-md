using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using PieroDeTomi.DotNetMd.Contracts.Services.Context;
using PieroDeTomi.DotNetMd.Contracts.Services.Generators;
using System.Text;

namespace PieroDeTomi.DotNetMd.Services.Generators.Microsoft
{
    internal class MsDocsGenerator : MarkdownGeneratorBase, IMarkdownDocsGenerator
    {
        private static readonly string _separator = $"{Environment.NewLine}{Environment.NewLine}";

        public MsDocsGenerator(DocGenerationRuntimeConfig configuration, IDocsGenerationContext context) : base(configuration, context) { }

        public string BuildMarkdown(TypeModel type)
        {
            List<string> docParts = [];

            docParts.Add(BuildHeader(type));

            if (type.HasInheritanceChain)
                docParts.Add(BuildInheritanceChain(type));

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
                    var propertyTypeReference = Context.FindObjectByIdentifier(p.Type.Identifier);
                    var propertyTypeLink = TryGetDocLink(propertyTypeReference, currentNamespace: type.Namespace);
                    var typeNameColumn = propertyTypeLink is not null ? $"[`{p.Type.Name}`]({propertyTypeLink})" : $"`{p.Type.Name}`";

                    return $"| `{p.Name}` | {typeNameColumn} | {GetSafeMarkdownText(p.Type.Summary, p.Type.Namespace, isTableCell: true)} |";
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
                        parameters += string.Join("<br />", m.Parameters.Select(parameter => $"- **`{parameter.Type}` {parameter.Name}:** {GetSafeMarkdownText(parameter.Description, parameter.Namespace)}"));
                    }
                    
                    return $"| `{m.GetSignature()}` | {GetSafeMarkdownText(m.Summary, m.Namespace, isTableCell: true)}{parameters} | {GetSafeMarkdownText(m.Returns, m.Namespace, isTableCell: true)} |";
                }));

                methods = MsDocsTemplatesProvider.Current.Methods.Replace(TemplateTokens.METHODS, methods);
                docParts.Add(methods);
            }

            return string.Join(_separator, docParts);
        }

        private string BuildHeader(TypeModel type)
        {
            return MsDocsTemplatesProvider.Current.Header
                .Replace(TemplateTokens.NAME, type.Name)
                .Replace(TemplateTokens.TYPE, type.ObjectType)
                .Replace(TemplateTokens.NAMESPACE, type.Namespace)
                .Replace(TemplateTokens.ASSEMBLY, type.Assembly)
                .Replace(TemplateTokens.DECLARATION, type.Declaration)
                .Replace(TemplateTokens.SUMMARY, type.Summary);
        }

        private string BuildInheritanceChain(TypeModel type)
        {
            var chain = new StringBuilder($"`{type.Name}` &rarr; ");

            chain.Append(string.Join(" &rarr; ", type.InheritanceChain.Select(baseType =>
            {
                var referenceType = Context.FindObjectByIdentifier(baseType.Identifier);

                if (referenceType is null) return $"`{baseType.Name}`";
                else return $"[`{baseType.Name}`]({TryGetDocLink(referenceType, currentNamespace: type.Namespace)})";
            })));

            return MsDocsTemplatesProvider.Current.Inheritance.Replace(TemplateTokens.INHERITANCE_CHAIN, chain.ToString());
        }
    }
}
