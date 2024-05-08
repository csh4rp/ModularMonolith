using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Mappings;

public class EntityMapping
{
    private static readonly Dictionary<Type, EntityMapping> Mappings = new();

    public string Container { get; protected set; }

    public bool IsAuditEnabled { get; protected set; }

    protected EntityMapping(string container) => Container = container;

    public static EntityMapping<T> For<T>(string container)
    {
        var mapping =  new EntityMapping<T>(container);
        Mappings[typeof(T)] = mapping;

        return mapping;
    }

    public static EntityMapping<T> Get<T>()
    {
        var type = typeof(T);
        if (Mappings.TryGetValue(type, out var mapping))
        {
            return (EntityMapping<T>)mapping;
        }

        return new EntityMapping<T>(type.Name);
    }
}

public class EntityMapping<T> : EntityMapping
{
    private Func<T, PartitionKey>? _partitionKeyExpression;

    public EntityMapping(string container) : base(container)
    {
    }

    public PartitionKey GetPartitionKey(T entity) => _partitionKeyExpression?.Invoke(entity) ?? new PartitionKey(typeof(T).Name);

    public EntityMapping<T> SetPartitionKey(Expression<Func<T, PartitionKey>> partitionKeyExpression)
    {
        _partitionKeyExpression = partitionKeyExpression.Compile();
        return this;
    }

    public EntityMapping<T> EnableAudit()
    {
        IsAuditEnabled = true;
        return this;
    }
}
