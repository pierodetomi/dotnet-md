using System.Reflection;

namespace PieroDeTomi.DotNetMd.Services
{
    public class DocTemplates
    {
        private static readonly DocTemplates _current;
        
        private readonly Assembly _assembly;

        public string DocusaurusCategory { get; private set; }
        
        public string DocusaurusFrontMatter { get; private set; }

        public string Header { get; private set; }
        
        public string TypeParams { get; private set; }
        
        public string TypeParam { get; private set; }
        
        public string Remarks { get; private set; }
        
        public string Properties { get; private set; }
        
        public string Methods { get; private set; }

        public static DocTemplates Current => _current;

        static DocTemplates()
        {
            _current = new();
        }

        private DocTemplates()
        {
            _assembly = Assembly.GetExecutingAssembly();

            DocusaurusCategory = GetTemplateContent($"{GetType().Namespace}.Resources.docusaurus-category.json");
            DocusaurusFrontMatter = GetTemplateContent($"{GetType().Namespace}.Resources.docusaurus-front-matter.md");
            Header = GetTemplateContent($"{GetType().Namespace}.Resources.template-header.md");
            TypeParams = GetTemplateContent($"{GetType().Namespace}.Resources.template-type-params.md");
            TypeParam = GetTemplateContent($"{GetType().Namespace}.Resources.template-type-param.md");
            Remarks = GetTemplateContent($"{GetType().Namespace}.Resources.template-remarks.md");
            Properties = GetTemplateContent($"{GetType().Namespace}.Resources.template-properties.md");
            Methods = GetTemplateContent($"{GetType().Namespace}.Resources.template-methods.md");
        }

        private string GetTemplateContent(string resourceName)
        {
            using Stream stream = _assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new(stream);
            
            return reader.ReadToEnd();
        }
    }
}