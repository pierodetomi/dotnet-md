namespace PieroDeTomi.DotNetMd.Services.Generators.Microsoft
{
    internal class MsDocsTemplatesProvider : TemplatesProviderBase
    {
        private static readonly MsDocsTemplatesProvider _current;

        public string Header { get; private set; }

        public string Inheritance { get; private set; }

        public string TypeParams { get; private set; }

        public string TypeParam { get; private set; }

        public string Remarks { get; private set; }

        public string Properties { get; private set; }

        public string Methods { get; private set; }

        public static MsDocsTemplatesProvider Current => _current;

        static MsDocsTemplatesProvider()
        {
            _current = new();
        }

        private MsDocsTemplatesProvider()
        {
            Header = GetTemplateContent($"{BaseNamespace}.Resources.MsDocs.template-header.md");
            Inheritance = GetTemplateContent($"{BaseNamespace}.Resources.MsDocs.template-inheritance.md");
            TypeParams = GetTemplateContent($"{BaseNamespace}.Resources.MsDocs.template-type-params.md");
            TypeParam = GetTemplateContent($"{BaseNamespace}.Resources.MsDocs.template-type-param.md");
            Remarks = GetTemplateContent($"{BaseNamespace}.Resources.MsDocs.template-remarks.md");
            Properties = GetTemplateContent($"{BaseNamespace}.Resources.MsDocs.template-properties.md");
            Methods = GetTemplateContent($"{BaseNamespace}.Resources.MsDocs.template-methods.md");
        }
    }
}