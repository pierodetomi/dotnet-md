using PieroDeTomi.DotNetMd.Contracts.Models;
using System.Reflection;
using System.Xml;

namespace PieroDeTomi.DotNetMd.Services.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static string GetIdentifier(this PropertyInfo property) => $"P:{property.DeclaringType.FullName}.{property.Name}";

        public static string GetDeclaration(this PropertyInfo property)
        {
            var getter = property.GetGetMethod(nonPublic: true);
            var setter = property.GetSetMethod(nonPublic: true);

            var type = property.PropertyType.GetDisplayName();
            var name = property.Name;

            var getterString = string.Empty;
            if (getter?.IsPublic ?? false) getterString = "get;";
            else if (getter?.IsPrivate ?? false) getterString = "private get;";

            var setterString = string.Empty;
            if (setter?.IsPublic ?? false) setterString = "set;";
            else if (setter?.IsPrivate ?? false) setterString = "private set;";

            var accessors = new List<string> { getterString, setterString };

            return getterString.Length == 0 && setterString.Length == 0
                ? $"{type} {name}"
                : $"{type} {name} {{ {string.Join(" ", accessors)} }}";
        }

        public static PropertyModel ToPropertyModel(this PropertyInfo property, TypeModel owner, XmlNode propertyXmlNode)
        {
            return new PropertyModel(owner)
            {
                Identifier = property.GetIdentifier(),
                Name = property.Name,
                Namespace = owner.Namespace,
                Assembly = owner.Assembly,
                Declaration = property.GetDeclaration(),
                Type = property.PropertyType.ToTypeModel(propertyXmlNode)
            };
        }
    }
}
