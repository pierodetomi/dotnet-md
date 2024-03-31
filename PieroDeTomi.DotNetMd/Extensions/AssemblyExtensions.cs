using System.Reflection;

namespace PieroDeTomi.DotNetMd.Extensions
{
    public static class AssemblyExtensions
    {
        public static List<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            try
            {
                return [.. assembly.GetTypes()];
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types
                    .Where(t => t is not null)
                    .ToList();
            }
        }
    }
}
