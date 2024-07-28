using System.Collections;
using System.Reflection;

namespace ModularMonolith.Shared.DataAccess.EntityFramework;

public class ConfigurationAssemblyCollection : IEnumerable<Assembly>
{
    private readonly IReadOnlyCollection<Assembly> _collection;

    private ConfigurationAssemblyCollection() => _collection = Array.Empty<Assembly>();

    private ConfigurationAssemblyCollection(IReadOnlyCollection<Assembly> collection)
    {
        if (collection.Count == 0)
        {
            throw new ArgumentException("Assembly collection cannot be empty");
        }

        _collection = collection;
    }
    
    public IEnumerator<Assembly> GetEnumerator() => _collection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public static ConfigurationAssemblyCollection Empty { get; } = new();
    
    public static ConfigurationAssemblyCollection FromAssemblies(IReadOnlyCollection<Assembly> collection) => 
        new(collection);
}
