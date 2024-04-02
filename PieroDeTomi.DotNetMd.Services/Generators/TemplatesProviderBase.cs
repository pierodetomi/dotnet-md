using System.Reflection;

namespace PieroDeTomi.DotNetMd.Services.Generators
{
    internal abstract class TemplatesProviderBase
    {
        private readonly Assembly _assembly;

        protected string BaseNamespace { get; private set; }

        public string DocusaurusCategory { get; private set; }

        public string DocusaurusFrontMatter { get; private set; }

        protected TemplatesProviderBase()
        {
            _assembly = Assembly.GetExecutingAssembly();

            BaseNamespace = typeof(DotNetMdTool).Namespace;

            DocusaurusCategory = GetTemplateContent($"{BaseNamespace}.Resources.Docusaurus.docusaurus-category.json");
            DocusaurusFrontMatter = GetTemplateContent($"{BaseNamespace}.Resources.Docusaurus.docusaurus-front-matter.md");
        }

        protected string GetTemplateContent(string resourceName)
        {
            using Stream stream = _assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new(stream);

            return reader.ReadToEnd();
        }
    }
}
