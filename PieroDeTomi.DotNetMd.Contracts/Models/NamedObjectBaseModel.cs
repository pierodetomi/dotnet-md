﻿namespace PieroDeTomi.DotNetMd.Contracts.Models
{
    public abstract class NamedObjectBaseModel
    {
        public abstract ObjectCategory ObjectCategory { get; }

        public string Name { get; set; }
        
        public string Namespace { get; set; }

        public string Assembly { get; set; }

        public string Summary { get; set; }

        public string Remarks { get; set; }
    }
}