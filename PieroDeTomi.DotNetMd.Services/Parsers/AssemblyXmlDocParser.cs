using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Docs;
using PieroDeTomi.DotNetMd.Services.Extensions;
using System.Reflection;
using System.Xml;

namespace PieroDeTomi.DotNetMd.Services.Parsers
{
    internal class AssemblyXmlDocParser(ILogger logger) : IAssemblyDocParser
    {
        public List<TypeModel> ParseTypes(string assemblyFilePath)
        {
            List<TypeModel> typeDescriptors = [];

            var (assembly, xmlDocument) = LoadAssembly(assemblyFilePath);

            assembly.GetLoadableTypes().ForEach(type =>
            {
                var typeIdentifier = $"T:{type.FullName}";
                var typeXmlNode = xmlDocument.SelectSingleNode($"/doc/members/member[@name='{typeIdentifier}']");

                if (typeXmlNode is null)
                    logger.LogWarning($"Unable to find XML documentation member for type {type.FullName}");

                var typeDescriptor = type.ToTypeModel(typeXmlNode);

                if (typeDescriptor is null)
                {
                    logger.LogError($"Unable to create type model for type {type.FullName}");
                    return;
                }

                type.GetProperties()
                    .Where(m => m.DeclaringType == type && !m.IsSpecialName)
                    .ToList()
                    .ForEach(propertyInfo =>
                    {
                        var propertyIdentifier = $"P:{propertyInfo.DeclaringType.FullName}.{propertyInfo.Name}";
                        var propertyXmlNode = xmlDocument.SelectSingleNode($"/doc/members/member[@name='{propertyIdentifier}']");

                        logger.LogWarning($"Unable to find XML documentation node for property {propertyInfo.Name}");

                        try
                        {
                            typeDescriptor.Properties.Add(new PropertyModel
                            {
                                Name = propertyInfo.Name,
                                Type = propertyInfo.PropertyType.ToTypeModel(propertyXmlNode)
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Unable to create property model for property {propertyInfo.Name} in type {type.Name}");
                        }
                    });

                type.GetMethods()
                    .Where(m => m.DeclaringType == type && !m.IsSpecialName)
                    .ToList()
                    .ForEach(methodInfo =>
                    {
                        var methodIdentifier = GetMethodIdentifier(methodInfo);
                        var methodXmlNode = methodIdentifier is not null ? xmlDocument.SelectSingleNode($"/doc/members/member[@name='{methodIdentifier}']") : null;

                        logger.LogWarning($"Unable to find XML documentation node for property {methodInfo.Name}");

                        var model = methodInfo.ToMethodModel(methodXmlNode);
                        
                        if (model is null)
                        {
                            logger.LogError($"Unable to create method model for method {methodInfo.Name}");
                            return;
                        }
                            
                        typeDescriptor.Methods.Add(methodInfo.ToMethodModel(methodXmlNode));
                    });

                typeDescriptors.Add(typeDescriptor);
            });

            return typeDescriptors;
        }

        private (Assembly assembly, XmlDocument xmlDocument) LoadAssembly(string assemblyFilePath)
        {
            if (!File.Exists(assemblyFilePath))
                throw new ApplicationException($"Assembly file {assemblyFilePath} does not exist.");

            var xmlDocFilePath = GetXmlDocFilePathFromAssemblyFilePath(assemblyFilePath);

            if (!File.Exists(xmlDocFilePath))
                throw new ApplicationException($"The specified assembly file does not appear to have an XML documentation file at {xmlDocFilePath}.");

            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyFilePath);

            // Load the XML document
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlDocFilePath);

            return (assembly, xmlDocument);
        }

        private static string GetXmlDocFilePathFromAssemblyFilePath(string assemblyFilePath)
        {
            var xmlFilePath = Path.GetDirectoryName(assemblyFilePath);
            return Path.Combine(xmlFilePath, $"{Path.GetFileNameWithoutExtension(assemblyFilePath)}.xml");
        }

        private string GetMethodIdentifier(MethodInfo methodInfo)
        {
            try
            {
                // Construct method identifier for generic methods
                var genericArguments = methodInfo.IsGenericMethod ? $"``{methodInfo.GetGenericArguments().Length}" : string.Empty;

                // Construct method identifier
                var parameters = string.Join(",", methodInfo.GetParameters().Select(p => GetParameterIdentifier(p.ParameterType)));
                return $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}{genericArguments}({parameters})";
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Error constructing method identifier for method {methodInfo.Name}");
                return null;
            }
        }

        private string GetParameterIdentifier(Type parameterType)
        {
            try
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
            catch(Exception ex)
            {
                logger.LogError(ex, $"Error constructing parameter identifier for type {parameterType.FullName}");
                return null;
            }
        }
    }
}
