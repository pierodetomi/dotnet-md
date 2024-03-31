﻿using PieroDeTomi.DotNetMd.Contracts.Docs;
using System.Reflection;
using System.Xml;

namespace PieroDeTomi.DotNetMd.Extensions
{
    public static class MethodInfoExtensions
    {
        public static string GetDisplayName(this MethodInfo method)
        {
            if (!method.IsGenericMethod)
                return method.Name;

            var genericMethod = method.GetGenericMethodDefinition();

            var typeArguments = string.Join(",", method.GetGenericArguments().Select(t => t.GetDisplayName()));

            return $"{genericMethod.Name}<{typeArguments}>";
        }

        public static MethodModel ToMethodModel(this MethodInfo method, XmlNode methodXmlNode)
        {
            var descriptor = new MethodModel
            {
                Name = method.GetDisplayName(),
                Namespace = method.DeclaringType.Namespace,
                Assembly = $"{method.DeclaringType.Assembly.GetName().Name}.dll",
                Summary = methodXmlNode?["summary"]?.InnerText?.Trim(),
                Remarks = methodXmlNode?["remarks"]?.InnerText?.Trim(),
                Returns = methodXmlNode?["returns"]?.InnerText?.Trim(),
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
                        Description = typeParamXmlNode?.InnerText?.Trim()
                    });
                });

            method.GetParameters().ToList().ForEach(parameter =>
            {
                descriptor.Parameters.Add(new ParamModel
                {
                    Type = parameter.ParameterType.GetDisplayName(),
                    Name = parameter.Name,
                    Description = methodXmlNode?.ChildNodes
                        .Cast<XmlNode>()
                        .FirstOrDefault(n => n.Name == "param" && n.Attributes["name"].Value == parameter.Name)?.InnerText?.Trim()
                });
            });

            return descriptor;
        }
    }
}
