namespace PieroDeTomi.DotNetMd.Extensions
{
    public static class StringExtensions
    {
        public static string MakeAbsolute(this string absoluteOrRelativePath, string basePath)
        {
            return Path.IsPathFullyQualified(absoluteOrRelativePath)
                ? absoluteOrRelativePath
                : Path.Combine(basePath, absoluteOrRelativePath);
        }
    }
}
