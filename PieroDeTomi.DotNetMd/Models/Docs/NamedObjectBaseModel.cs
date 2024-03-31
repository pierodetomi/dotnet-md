namespace PieroDeTomi.DotNetMd.Models.Docs
{
    public abstract class NamedObjectBaseModel
    {
        public string Name { get; set; }
        
        public string Namespace { get; set; }

        public string Assembly { get; set; }

        public string Summary { get; set; }

        public string Remarks { get; set; }
    }
}