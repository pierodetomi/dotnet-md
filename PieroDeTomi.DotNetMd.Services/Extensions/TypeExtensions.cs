using PieroDeTomi.DotNetMd.Contracts.Models;
using System.Xml;

namespace PieroDeTomi.DotNetMd.Services.Extensions
{
    public static class TypeExtensions
    {
        public static TypeModel ToTypeModel(this Type type, XmlNode xmlDocNode = null)
        {
            try
            {
                var descriptor = new TypeModel
                {
                    InheritanceChain = type.GetInheritanceChain(),
                    ObjectType = type.IsClass ? "class" : type.IsInterface ? "interface" : type.IsEnum ? "enum" : "struct",
                    Declaration = type.GetDeclaration(),
                    Name = type.GetDisplayName(),
                    Namespace = type.Namespace,
                    Assembly = $"{type.Assembly.GetName().Name}.dll",
                    Summary = xmlDocNode?["summary"]?.InnerXml?.Trim(),
                    Remarks = xmlDocNode?["remarks"]?.InnerXml?.Trim()
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
                            Description = typeParamXmlNode?.InnerXml?.Trim()
                        });
                    });

                return descriptor;
            }
            catch
            {
                return null;
            }
        }

        public static string GetDisplayName(this Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericType = type.GetGenericTypeDefinition();

            var name = genericType.FullName.Split('.').Last();
            var nameWithoutArguments = name.Substring(0, name.IndexOf('`'));
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

        public static List<TypeModel> GetInheritanceChain(this Type type)
        {
            List<TypeModel> chain = [];

            var currentType = type;

            while (currentType.BaseType is not null)
            {
                chain.Add(currentType.BaseType.ToTypeModel());
                currentType = currentType.BaseType;
            }

            return chain;
        }
    }
}
