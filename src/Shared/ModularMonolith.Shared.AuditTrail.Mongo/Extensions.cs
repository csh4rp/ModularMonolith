using ModularMonolith.Shared.AuditTrail.Mongo.Mapping;
using MongoDB.Bson.Serialization;

namespace ModularMonolith.Shared.AuditTrail.Mongo;

public static class Extensions
{
    public static string? GetCollectionName(this BsonClassMap classMap)
    {
        var entityClassMap = classMap as IEntityClassMap;
        return entityClassMap?.CollectionName ?? classMap.ClassType.Name;
    }

    public static bool UsesAuditLog(this BsonClassMap classMap)
    {
        var entityClassMap = classMap as IEntityClassMap;
        return entityClassMap?.UsesAuditTrail is true;
    }
}
