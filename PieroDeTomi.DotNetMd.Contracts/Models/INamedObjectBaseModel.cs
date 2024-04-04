namespace PieroDeTomi.DotNetMd.Contracts.Models
{
    public interface INamedObjectBaseModel
    {
        ObjectCategory ObjectCategory { get; }

        TypeModel Owner { get; }

        string Identifier { get; }

        string Name { get; }
        
        string Namespace { get; }

        string Assembly { get; }

        string Summary { get; }

        string Remarks { get; }
    }
}