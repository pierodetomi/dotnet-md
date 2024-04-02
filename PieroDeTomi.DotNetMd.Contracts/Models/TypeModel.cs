namespace PieroDeTomi.DotNetMd.Contracts.Models
{
    public class TypeModel : NamedObjectBaseModel
    {
        public string ObjectType { get; set; }
        
        public string Declaration { get; set; }
        
        public List<PropertyModel> Properties { get; set; } = [];

        public List<ParamModel> TypeParameters { get; set; } = [];

        public List<MethodModel> Methods { get; set; } = [];

    }
}