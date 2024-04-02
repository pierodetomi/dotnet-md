using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Models.Config;
using PieroDeTomi.DotNetMd.Contracts.Services.Generators;

namespace PieroDeTomi.DotNetMd.Services.Generators.Default
{
    internal class DefaultGenerator : MarkdownGeneratorBase, IMarkdownDocsGenerator
    {
        private static readonly string _separator = $"{Environment.NewLine}{Environment.NewLine}";

        public DefaultGenerator(DocGenerationRuntimeConfig configuration) : base(configuration) { }

        public string BuildMarkdown(TypeModel type, List<TypeModel> allTypes)
        {
            List<string> docParts = [];

            docParts.Add(DefaultTemplatesProvider.Current.Header
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

            if (type.HasTypeParameters)
            {
                docParts.Add("## Type Parameters");

                docParts.Add(string.Join(_separator, type.TypeParameters.Select(tp =>
                    DefaultTemplatesProvider.Current.GenericParameter
                        .Replace(TemplateTokens.NAME, tp.Name)
                        .Replace(TemplateTokens.DESCRIPTION, tp.Description))));
            }

            if (type.Remarks is not null)
            {
                docParts.Add($"## Remarks");
                docParts.Add(type.Remarks);
            }

            if (type.HasProperties)
            {
                docParts.Add("## Properties");

                type.Properties.ForEach(property =>
                {
                    docParts.Add(DefaultTemplatesProvider.Current.Parameter
                        .Replace(TemplateTokens.NAME, property.Name)
                        .Replace(TemplateTokens.TYPE, property.Type.Name)
                        .Replace(TemplateTokens.DESCRIPTION, property.Type.Summary));
                });
            }

            if (type.HasMethods)
            {
                docParts.Add("## Methods");

                type.Methods.ForEach(method =>
                {
                    docParts.Add(DefaultTemplatesProvider.Current.Method
                        .Replace(TemplateTokens.NAME, method.Name.Replace("<", "&lt;").Replace(">", "&gt;"))
                        .Replace(TemplateTokens.SUMMARY, GetSafeMarkdownText(method.Summary))
                        .Replace(TemplateTokens.DECLARATION, method.GetSignature()));

                    if (method.HasParameters)
                    {
                        docParts.Add("#### Parameters");

                        method.Parameters.ForEach(parameter =>
                        {
                            docParts.Add(DefaultTemplatesProvider.Current.Parameter
                                .Replace(TemplateTokens.NAME, parameter.Name)
                                .Replace(TemplateTokens.TYPE, parameter.Type)
                                .Replace(TemplateTokens.DESCRIPTION, parameter.Description));
                        });
                    }

                    if (method.ReturnType.Name.ToLower() != "void")
                    {
                        docParts.Add("#### Returns");
                        
                        docParts.Add($"`{method.ReturnType.Name}`");

                        if (method.Returns is not null)
                            docParts.Add(GetSafeMarkdownText(method.Returns));
                    }
                });
            }

            return string.Join(_separator, docParts);
        }
    }
}
