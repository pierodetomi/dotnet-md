namespace PieroDeTomi.DotNetMd.Contracts.Models
{
    public class TypeModel : NamedObjectBaseModel, INamedObjectBaseModel
    {
        public override ObjectCategory ObjectCategory => ObjectCategory.Type;

        public TypeModel Owner { get; private set; } = null;

        public string Identifier { get; set; }

        public string ObjectType { get; set; }
        
        public string Declaration { get; set; }

        public List<TypeModel> InheritanceChain { get; set; } = [];

        public List<PropertyModel> Properties { get; set; } = [];

        public List<ParamModel> TypeParameters { get; set; } = [];

        public List<MethodModel> Methods { get; set; } = [];

        public bool HasInheritanceChain => InheritanceChain.Count > 0;

        public bool HasTypeParameters => TypeParameters.Count > 0;

        public bool HasProperties => Properties.Count > 0;

        public bool HasMethods => Methods.Count > 0;
    }
}