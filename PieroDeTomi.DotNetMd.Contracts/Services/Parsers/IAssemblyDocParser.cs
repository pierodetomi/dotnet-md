using PieroDeTomi.DotNetMd.Contracts.Models;

namespace PieroDeTomi.DotNetMd.Contracts.Services.Parsers
{
    public interface IAssemblyDocParser
    {
        List<TypeModel> ParseTypes(string assemblyFilePath);
    }
}
