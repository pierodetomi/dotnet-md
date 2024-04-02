namespace PieroDeTomi.DotNetMd.Contracts.Docs
{
    public interface IAssemblyDocParser
    {
        List<TypeModel> ParseTypes(string assemblyFilePath);
    }
}
