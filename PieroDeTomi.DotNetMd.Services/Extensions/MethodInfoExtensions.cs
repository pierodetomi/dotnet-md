using PieroDeTomi.DotNetMd.Contracts.Models;
using System.Reflection;
using System.Xml;

namespace PieroDeTomi.DotNetMd.Services.Extensions
{
    public static class MethodInfoExtensions
    {
        public static string GetIdentifier(this MethodInfo methodInfo)
        {
            try
            {
                // Construct method identifier for generic methods
                var genericArguments = methodInfo.IsGenericMethod ? $"``{methodInfo.GetGenericArguments().Length}" : string.Empty;

                // Construct method identifier
                var parameters = string.Join(",", methodInfo.GetParameters().Select(p => GetParameterIdentifier(p.ParameterType)));
                return $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}{genericArguments}({parameters})";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetDisplayName(this MethodInfo method)
        {
            if (!method.IsGenericMethod)
                return method.Name;

            var genericMethod = method.GetGenericMethodDefinition();

            var typeArguments = string.Join(",", method.GetGenericArguments().Select(t => t.GetDisplayName()));

            return $"{genericMethod.Name}<{typeArguments}>";
        }

        public static MethodModel ToMethodModel(this MethodInfo method, TypeModel owner, XmlNode methodXmlNode)
        {
            try
            {
                var descriptor = new MethodModel(owner)
                {
                    Identifier = method.GetIdentifier(),
                    Name = method.GetDisplayName(),
                    Namespace = method.DeclaringType.Namespace,
                    Assembly = $"{method.DeclaringType.Assembly.GetName().Name}.dll",
                    Summary = methodXmlNode?["summary"]?.InnerXml?.Trim(),
                    Remarks = methodXmlNode?["remarks"]?.InnerXml?.Trim(),
                    Returns = methodXmlNode?["returns"]?.InnerXml?.Trim(),
                    ReturnType = method.ReturnType.ToTypeModel()
                };

                if (method.IsGenericMethod)
                    method.GetGenericArguments().ToList().ForEach(genericArgument =>
                    {
                        var typeParamXmlNode = methodXmlNode?.ChildNodes
                            .Cast<XmlNode>()
                            .FirstOrDefault(n => n.Name == "typeparam" && n.Attributes["name"].Value == genericArgument.Name);

                        descriptor.TypeParameters.Add(new ParamModel
                        {
                            Name = genericArgument.Name,
                            Namespace = genericArgument.Namespace ?? descriptor.Namespace,
                            Description = typeParamXmlNode?.InnerXml?.Trim(),
                        });
                    });

                method.GetParameters().ToList().ForEach(parameter =>
                {
                    descriptor.Parameters.Add(new ParamModel
                    {
                        Type = parameter.ParameterType.GetDisplayName(),
                        Name = parameter.Name,
                        Namespace = descriptor.Namespace,
                        Description = methodXmlNode?.ChildNodes
                            .Cast<XmlNode>()
                            .FirstOrDefault(n => n.Name == "param" && n.Attributes["name"].Value == parameter.Name)?.InnerXml?.Trim()
                    });
                });

                return descriptor;
            }
            catch
            {
                return null;
            }
        }

        private static string GetParameterIdentifier(Type parameterType)
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
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
