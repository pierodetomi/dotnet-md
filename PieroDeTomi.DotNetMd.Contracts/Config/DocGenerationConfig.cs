namespace PieroDeTomi.DotNetMd.Contracts.Config
{
    public class DocGenerationConfig
    {
        public List<string> Assemblies { get; set; } = [];

        public string OutputPath { get; set; } = @".\docs";

        public string OutputStyle { get; set; } = "default"; // default | "microsoft"

        public bool ShouldCreateNamespaceFolders { get; set; } = true;

        public bool IsDocusaurusProject { get; set; } = false;
    }
}