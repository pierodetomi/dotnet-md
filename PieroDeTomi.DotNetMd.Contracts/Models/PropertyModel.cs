namespace PieroDeTomi.DotNetMd.Contracts.Models
{
    public class PropertyModel : INamedObjectBaseModel
    {
        public ObjectCategory ObjectCategory => ObjectCategory.Property;

        public TypeModel Owner { get; private set; }

        public string Identifier { get; set; }

        public string Declaration { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Assembly { get; set; }

        public TypeModel Type { get; set; }

        public string Summary => Type?.Summary;

        public string Remarks => Type?.Remarks;

        public PropertyModel(TypeModel owner)
        {
            Owner = owner;
        }
    }
}