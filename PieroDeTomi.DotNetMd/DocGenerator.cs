using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Models.Docs;

namespace PieroDeTomi.DotNetMd
{
    public class DocGenerator(ILogger logger)
    {
        private static readonly string _separator = $"{Environment.NewLine}{Environment.NewLine}";

        public string GenerateDoc(TypeModel type)
        {
            List<string> docParts = [];

            var header = DocTemplates.Current.Header
                .Replace("{{NAME}}", type.Name)
                .Replace("{{TYPE}}", type.ObjectType)
                .Replace("{{NAMESPACE}}", type.Namespace)
                .Replace("{{ASSEMBLY}}", type.Assembly)
                .Replace("{{DECLARATION}}", type.Declaration)
                .Replace("{{SUMMARY}}", type.Summary);

            docParts.Add(header);

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
            {
                var remarks = DocTemplates.Current.Remarks.Replace("{{REMARKS}}", type.Remarks);
                docParts.Add(remarks);
            }

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
    }
}
