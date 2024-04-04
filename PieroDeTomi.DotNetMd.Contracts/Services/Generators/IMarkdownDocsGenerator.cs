using PieroDeTomi.DotNetMd.Contracts.Models;

namespace PieroDeTomi.DotNetMd.Contracts.Services.Generators
{
    public interface IMarkdownDocsGenerator
    {
        string GetSanitizedFileName(string name);

        void WriteDocusaurusCategoryFile(string folderPath, string label, int position, string description = null);

        string BuildMarkdown(TypeModel type);
    }
}