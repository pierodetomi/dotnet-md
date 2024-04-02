namespace PieroDeTomi.DotNetMd.Contracts.Models
{
    public class TypeModel : NamedObjectBaseModel
    {
        public string ObjectType { get; set; }
        
        public string Declaration { get; set; }

        public List<string> InheritanceChain { get; set; } = [];

        public List<PropertyModel> Properties { get; set; } = [];

        public List<ParamModel> TypeParameters { get; set; } = [];

        public List<MethodModel> Methods { get; set; } = [];

        public bool HasTypeParameters => TypeParameters.Count > 0;

        public bool HasProperties => Properties.Count > 0;

        public bool HasMethods => Methods.Count > 0;
    }
}