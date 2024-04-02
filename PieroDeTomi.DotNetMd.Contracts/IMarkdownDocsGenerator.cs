using PieroDeTomi.DotNetMd.Contracts.Docs;

namespace PieroDeTomi.DotNetMd.Contracts
{
    public interface IMarkdownDocsGenerator
    {
        string GetSanitizedFileName(string name);

        void WriteDocusaurusCategoryFile(string folderPath, string label, int position, string description = null);

        string BuildMarkdown(TypeModel type, List<TypeModel> allTypes);
    }
}