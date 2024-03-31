﻿namespace PieroDeTomi.DotNetMd.Models.Docs
{
    public class MethodModel : NamedObjectBaseModel
    {
        public string Returns { get; set; }

        public TypeModel ReturnType { get; set; }

        public List<ParamModel> TypeParameters { get; set; } = [];

        public List<ParamModel> Parameters { get; set; } = [];

        public string GetSignature()
        {
            var returnType = ReturnType.Name == "Void" ? string.Empty : $"{ReturnType.Name} ";
            return $"{returnType}{Name}({string.Join(", ", Parameters.Select(p => $"{p.Type} {p.Name}"))})";
        }
    }
}