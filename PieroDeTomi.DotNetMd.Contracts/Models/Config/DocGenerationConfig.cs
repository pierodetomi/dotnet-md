using PieroDeTomi.DotNetMd.Contracts.Models.Theming;

namespace PieroDeTomi.DotNetMd.Contracts.Models.Config
{
    public class DocGenerationConfig
    {
        public List<string> Assemblies { get; set; } = [];

        public string OutputPath { get; set; } = @".\docs";

        public string OutputStyle { get; set; } = OutputStyles.Default; // OutputStyles.Default | OutputStyles.Microsoft

        public bool ShouldCreateNamespaceFolders { get; set; } = true;

        public bool IsDocusaurusProject { get; set; } = false;

        public DocusaurusOptions DocusaurusOptions { get; set; }
    }
}