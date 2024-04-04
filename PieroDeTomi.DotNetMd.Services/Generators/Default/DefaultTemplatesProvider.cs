namespace PieroDeTomi.DotNetMd.Services.Generators.Default
{
    internal class DefaultTemplatesProvider : TemplatesProviderBase
    {
        private static readonly DefaultTemplatesProvider _current;

        public string Header { get; private set; }

        public string Inheritance { get; private set; }
        
        public string Property { get; private set; }
        
        public string Method { get; private set; }
        
        public string MethodRemarks { get; private set; }
        
        public string GenericParameter { get; private set; }
        
        public string Parameter { get; private set; }

        public static DefaultTemplatesProvider Current => _current;

        static DefaultTemplatesProvider()
        {
            _current = new();
        }

        private DefaultTemplatesProvider() : base()
        {
            Header = GetTemplateContent($"{BaseNamespace}.Resources.Default.template-header.md");
            Inheritance = GetTemplateContent($"{BaseNamespace}.Resources.Default.template-inheritance.md");
            Property = GetTemplateContent($"{BaseNamespace}.Resources.Default.template-property.md");
            Method = GetTemplateContent($"{BaseNamespace}.Resources.Default.template-method.md");
            MethodRemarks = GetTemplateContent($"{BaseNamespace}.Resources.Default.template-method-remarks.md");
            GenericParameter = GetTemplateContent($"{BaseNamespace}.Resources.Default.template-generic-parameter.md");
            Parameter = GetTemplateContent($"{BaseNamespace}.Resources.Default.template-parameter.md");
        }
    }
}