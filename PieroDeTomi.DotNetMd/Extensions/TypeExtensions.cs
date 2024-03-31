using PieroDeTomi.DotNetMd.Contracts.Docs;
using System.Xml;

namespace PieroDeTomi.DotNetMd.Extensions
{
    public static class TypeExtensions
    {
        public static TypeModel ToTypeModel(this Type type, XmlNode xmlDocNode = null)
        {
            var descriptor = new TypeModel
            {
                ObjectType = type.IsClass ? "class" : type.IsInterface ? "interface" : type.IsEnum ? "enum" : "struct",
                Declaration = type.GetDeclaration(),
                Name = type.GetDisplayName(),
                Namespace = type.Namespace,
                Assembly = $"{type.Assembly.GetName().Name}.dll",
                Summary = xmlDocNode?["summary"]?.InnerText?.Trim(),
                Remarks = xmlDocNode?["remarks"]?.InnerText?.Trim()
            };

            if (type.IsGenericType)
                type.GetGenericArguments().ToList().ForEach(genericArgument =>
                {
                    var typeParamXmlNode = xmlDocNode?.ChildNodes
                        .Cast<XmlNode>()
                        .FirstOrDefault(n => n.Name == "typeparam" && n.Attributes["name"].Value == genericArgument.Name);

                    descriptor.TypeParameters.Add(new ParamModel
                    {
                        Name = genericArgument.Name,
                        Description = typeParamXmlNode?.InnerText?.Trim()
                    });
                });

            return descriptor;
        }

        public static string GetDisplayName(this Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericType = type.GetGenericTypeDefinition();

            var nameWithoutArguments = genericType.Name.Substring(0, genericType.Name.IndexOf('`'));
            var typeArguments = string.Join(",", type.GetGenericArguments().Select(GetDisplayName));

            return $"{nameWithoutArguments}<{typeArguments}>";
        }

        public static string GetDeclaration(this Type type)
        {
            var objectType = string.Empty;

            if (type.IsClass)
                objectType = "class";
            else if (type.IsInterface)
                objectType = "interface";
            else if (type.IsEnum)
                objectType = "enum";
            else if (type.IsValueType)
                objectType = "struct";

            var displayName = type.GetDisplayName();
            var baseTypeName = type.BaseType == typeof(object) ? null : (type.BaseType?.GetDisplayName() ?? null);
            var interfaceNames = type.GetInterfaces().Select(i => i.GetDisplayName());

            var declaration = $"{objectType} {displayName} : {(baseTypeName is null ? string.Empty : $"{baseTypeName}, ")}{string.Join(", ", interfaceNames)}";

            if (declaration.EndsWith(" : "))
                declaration = declaration.Replace(" : ", string.Empty);

            return declaration;
        }
    }
}
