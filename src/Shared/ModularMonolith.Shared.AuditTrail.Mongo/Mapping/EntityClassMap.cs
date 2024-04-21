using MongoDB.Bson.Serialization;

namespace ModularMonolith.Shared.AuditTrail.Mongo.Mapping;

public class EntityClassMap<T> : BsonClassMap<T>, IEntityClassMap
{
    public string CollectionName { get; private set; } = typeof(T).Name;

    public bool UsesAuditTrail { get; private set; }

    public void SetCollectionName(string collectionName) => CollectionName = collectionName;

    private void UseAuditTrail() => UsesAuditTrail = true;
}
