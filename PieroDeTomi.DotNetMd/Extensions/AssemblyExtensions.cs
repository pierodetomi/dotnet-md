using System.Reflection;

namespace PieroDeTomi.DotNetMd.Extensions
{
    public static class AssemblyExtensions
    {
        public static List<Type> GetLoadableTypes(this Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            try
            {
                return assembly.GetTypes().ToList();
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
