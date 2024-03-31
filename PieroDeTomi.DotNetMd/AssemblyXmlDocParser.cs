using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Docs;
using PieroDeTomi.DotNetMd.Extensions;
using System.Reflection;
using System.Xml;

namespace PieroDeTomi.DotNetMd
{
    internal class AssemblyXmlDocParser(ILogger logger)
    {
        private Assembly _assembly;

        private XmlDocument _xmlDocument;

        public void LoadAssembly(string assemblyFilePath)
        {
            if (!File.Exists(assemblyFilePath))
                throw new ApplicationException($"Assembly file {assemblyFilePath} does not exist.");

            var xmlDocFilePath = GetXmlDocFilePathFromAssemblyFilePath(assemblyFilePath);

            if (!File.Exists(xmlDocFilePath))
                throw new ApplicationException($"The specified assembly file does not appear to have an XML documentation file at {xmlDocFilePath}.");

            // Load the assembly
            _assembly = Assembly.LoadFrom(assemblyFilePath);
            
            // Load the XML document
            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(xmlDocFilePath);
        }

        public List<TypeModel> GetTypes()
        {
            List<TypeModel> typeDescriptors = [];

            _assembly.GetLoadableTypes().ForEach(type =>
            {
                var typeIdentifier = $"T:{type.FullName}";
                var typeXmlNode = _xmlDocument.SelectSingleNode($"/doc/members/member[@name='{typeIdentifier}']");

                if (typeXmlNode is null)
                {
                    logger.LogWarning($"Unable to find XML documentation member for type {type.FullName}");
                    return;
                }

                var typeDescriptor = type.ToTypeModel(typeXmlNode);

                type.GetProperties()
                    .Where(m => m.DeclaringType == type && !m.IsSpecialName)
                    .ToList()
                    .ForEach(propertyInfo =>
                    {
                        var propertyIdentifier = $"P:{propertyInfo.DeclaringType.FullName}.{propertyInfo.Name}";
                        var propertyXmlNode = _xmlDocument.SelectSingleNode($"/doc/members/member[@name='{propertyIdentifier}']");

                        logger.LogWarning($"Unable to find XML documentation node for property {propertyInfo.Name}");

                        typeDescriptor.Properties.Add(new PropertyModel
                        {
                            Name = propertyInfo.Name,
                            Type = propertyInfo.PropertyType.ToTypeModel(propertyXmlNode)
                        });
                    });

                type.GetMethods()
                    .Where(m => m.DeclaringType == type && !m.IsSpecialName)
                    .ToList()
                    .ForEach(methodInfo =>
                    {
                        var methodIdentifier = GetMethodIdentifier(methodInfo);
                        var methodXmlNode = _xmlDocument.SelectSingleNode($"/doc/members/member[@name='{methodIdentifier}']");

                        logger.LogWarning($"Unable to find XML documentation node for property {methodInfo.Name}");

                        typeDescriptor.Methods.Add(methodInfo.ToMethodModel(methodXmlNode));
                    });

                typeDescriptors.Add(typeDescriptor);
            });

            return typeDescriptors;
        }

        private string GetTypeDisplayName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericType = type.GetGenericTypeDefinition();
            
            var nameWithoutArguments = genericType.Name.Substring(0, genericType.Name.IndexOf('`'));
            var typeArguments = string.Join(",", type.GetGenericArguments().Select(GetTypeDisplayName));

            return $"{nameWithoutArguments}<{typeArguments}>";
        }

        private static string GetXmlDocFilePathFromAssemblyFilePath(string assemblyFilePath)
        {
            var xmlFilePath = Path.GetDirectoryName(assemblyFilePath);
            return Path.Combine(xmlFilePath, $"{Path.GetFileNameWithoutExtension(assemblyFilePath)}.xml");
        }

        private static string GetMethodIdentifier(MethodInfo methodInfo)
        {
            // Construct method identifier for generic methods
            var genericArguments = methodInfo.IsGenericMethod ? $"``{methodInfo.GetGenericArguments().Length}" : string.Empty;

            // Construct method identifier
            var parameters = string.Join(",", methodInfo.GetParameters().Select(p => GetParameterIdentifier(p.ParameterType)));
            return $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}{genericArguments}({parameters})";
        }

        private static string GetParameterIdentifier(Type parameterType)
        {
            if (parameterType.IsGenericMethodParameter)
                return $"``{parameterType.GenericParameterPosition}";

            else if (parameterType.IsGenericTypeParameter)
                return $"`{parameterType.GenericParameterPosition}";

            else if (!parameterType.IsGenericType)
                return parameterType.FullName;

            // For generic parameters, recursively construct the parameter identifier
            var typeName = parameterType.GetGenericTypeDefinition().FullName;
            var typeArguments = string.Join(",", parameterType.GetGenericArguments().Select(GetParameterIdentifier));
            
            return $"{typeName}{{{typeArguments}}}";
        }
    }
}
