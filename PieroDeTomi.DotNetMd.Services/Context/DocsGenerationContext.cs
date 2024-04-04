using PieroDeTomi.DotNetMd.Contracts.Models;
using PieroDeTomi.DotNetMd.Contracts.Services.Context;
using System.Collections.Concurrent;

namespace PieroDeTomi.DotNetMd.Services.Context
{
    internal class DocsGenerationContext : IDocsGenerationContext
    {
        private static readonly ConcurrentBag<TypeModel> _types = [];

        private static readonly ConcurrentDictionary<string, INamedObjectBaseModel> _objectsByIdentifier = [];

        public void SetTypes(List<TypeModel> types)
        {
            _types.Clear();
            _objectsByIdentifier.Clear();

            types.ForEach(AddType);
        }

        public void AddType(TypeModel type)
        {
            if (_types.Any(t => t.Identifier == type.Identifier))
                return;

            _types.Add(type);
            _objectsByIdentifier[type.Identifier] = type;

            type.Properties?.ForEach(property => _objectsByIdentifier[property.Identifier] = property);
            type.Methods?.ForEach(method => _objectsByIdentifier[method.Identifier] = method);
        }

        public INamedObjectBaseModel FindObjectByIdentifier(string identifier) => _objectsByIdentifier.ContainsKey(identifier) ? _objectsByIdentifier[identifier] : null;
    }
}
