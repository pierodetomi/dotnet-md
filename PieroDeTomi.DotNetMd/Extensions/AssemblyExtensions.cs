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
                var publicTypes = assembly.GetTypes();
                return Array.FindAll(publicTypes, t => t.IsPublic && t.Assembly == assembly).ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types
                    .Where(t => t is not null && t.IsPublic && t.Assembly == assembly)
                    .ToList();
            }
        }
    }
}
