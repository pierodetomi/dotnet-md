using PieroDeTomi.DotNetMd.Contracts.Models;

namespace PieroDeTomi.DotNetMd.Contracts.Services.Emitters
{
    public interface IDocsEmitter
    {
        void Emit(List<TypeModel> types);
    }
}
