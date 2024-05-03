using MongoDB.Bson.Serialization;

namespace ModularMonolith.Shared.DataAccess.Mongo;

public static class MappingExtensions
{
    private class MongoMappingProperties
    {
        public string? CollectionName { get; set; }
        
        public bool UsesAuditLog { get; set; }
    }
    
    
    private static readonly Dictionary<BsonClassMap, MongoMappingProperties> MappingProps = new();
    
    public static BsonClassMap MapCollectionName(this BsonClassMap classMap, string collectionName)
    {
        ArgumentException.ThrowIfNullOrEmpty(collectionName);
        
        if (MappingProps.TryGetValue(classMap, out var props))
        {
            props.CollectionName = collectionName;
        }
        else
        {
            MappingProps[classMap] = new MongoMappingProperties
            {
                CollectionName = collectionName
            };
        }
        
        return classMap;
    }
    
    public static string GetCollectionName(this BsonClassMap classMap) =>
        MappingProps.TryGetValue(classMap, out var props) && props.CollectionName is not null
            ? props.CollectionName
            : classMap.ClassType.Name;

    public static BsonClassMap UseAuditLog(this BsonClassMap classMap)
    {
        if (MappingProps.TryGetValue(classMap, out var props))
        {
            props.UsesAuditLog = true;
        }
        else
        {
            MappingProps[classMap] = new MongoMappingProperties
            {
                UsesAuditLog = true
            };
        }
        
        return classMap;
    }
    
    public static bool IsAuditable(this BsonClassMap classMap) =>
        MappingProps.TryGetValue(classMap, out var props) && props.UsesAuditLog;
}
