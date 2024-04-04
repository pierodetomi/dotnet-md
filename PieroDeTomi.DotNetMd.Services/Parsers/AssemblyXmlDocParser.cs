using Microsoft.Extensions.Logging;
using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Services.Parsers;
using PieroDeTomi.DotNetMd.Services.Extensions;
using System.Reflection;
using System.Xml;

namespace PieroDeTomi.DotNetMd.Services.Parsers
{
    internal class AssemblyXmlDocParser : IAssemblyDocParser
    {
        private readonly ILogger _logger;

        public AssemblyXmlDocParser(ILogger logger)
        {
            _logger = logger;
        }

        public List<TypeModel> ParseTypes(string assemblyFilePath)
        {
            List<TypeModel> typeDescriptors = [];

            var (assembly, xmlDocument) = LoadAssembly(assemblyFilePath);

            assembly.GetLoadableTypes().ForEach(type =>
            {
                var typeIdentifier = type.GetIdentifier();
                var typeXmlNode = xmlDocument.SelectSingleNode($"/doc/members/member[@name='{typeIdentifier}']");

                if (typeXmlNode is null)
                    _logger.LogWarning($"Unable to find XML documentation member for type {type.FullName}");

                var typeDescriptor = type.ToTypeModel(typeXmlNode);

                if (typeDescriptor is null)
                {
                    _logger.LogError($"Unable to create type model for type {type.FullName}");
                    return;
                }

                type.GetProperties()
                    .Where(m => m.DeclaringType == type && !m.IsSpecialName)
                    .ToList()
                    .ForEach(propertyInfo =>
                    {
                        var propertyIdentifier = propertyInfo.GetIdentifier();
                        var propertyXmlNode = xmlDocument.SelectSingleNode($"/doc/members/member[@name='{propertyIdentifier}']");

                        if (propertyXmlNode is null)
                            _logger.LogWarning($"Unable to find XML documentation node for property {propertyInfo.Name}");

                        try
                        {
                            typeDescriptor.Properties.Add(propertyInfo.ToPropertyModel(typeDescriptor, propertyXmlNode));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Unable to create property model for property {propertyInfo.Name} in type {type.Name}");
                        }
                    });

                type.GetMethods()
                    .Where(m => m.DeclaringType == type && !m.IsSpecialName)
                    .ToList()
                    .ForEach(methodInfo =>
                    {
                        var methodIdentifier = methodInfo.GetIdentifier();

                        if (methodIdentifier is null)
                        {
                            _logger.LogError($"Error constructing method identifier for method {methodInfo.Name}");
                            return;
                        }

                        var methodXmlNode = methodIdentifier is not null ? xmlDocument.SelectSingleNode($"/doc/members/member[@name='{methodIdentifier}']") : null;

                        if (methodXmlNode is null)
                            _logger.LogWarning($"Unable to find XML documentation node for method {methodInfo.Name}");

                        var model = methodInfo.ToMethodModel(typeDescriptor, methodXmlNode);
                        
                        if (model is null)
                        {
                            _logger.LogError($"Unable to create method model for method {methodInfo.Name}");
                            return;
                        }
                            
                        typeDescriptor.Methods.Add(model);
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
    }
}
