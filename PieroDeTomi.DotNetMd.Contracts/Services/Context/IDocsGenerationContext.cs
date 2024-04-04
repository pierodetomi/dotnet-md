using PieroDeTomi.DotNetMd.Contracts.Models;

namespace PieroDeTomi.DotNetMd.Contracts.Services.Context
{
    public interface IDocsGenerationContext
    {
        void SetTypes(List<TypeModel> types);
        
        void AddType(TypeModel type);

        INamedObjectBaseModel FindObjectByIdentifier(string identifier);
    }
}
