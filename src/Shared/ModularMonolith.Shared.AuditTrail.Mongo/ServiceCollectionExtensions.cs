using ModularMonolith.Shared.AuditTrail.Mongo.Mapping;
using ModularMonolith.Shared.AuditTrail.Mongo.Model;
using ModularMonolith.Shared.AuditTrail.Mongo.Options;
using MongoDB.Bson.Serialization;

namespace ModularMonolith.Shared.AuditTrail.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoAuditTrail(this IServiceCollection serviceCollection,
        Action<AuditTrailOptions> optionsAction)
    {
        var options = new AuditTrailOptions();
        optionsAction(options);

        var classMap = new EntityClassMap<AuditLogEntity>();
        classMap.MapIdMember(b => b.Id);
        classMap.MapProperty(b => b.EntityPropertyChanges).SetElementName("entity_property_changes");
        classMap.MapProperty(b => b.EntityKeys).SetElementName("entity_keys");
        classMap.MapProperty(b => b.EntityState).SetElementName("entity_state");
        classMap.MapProperty(b => b.EntityType).SetElementName("entity_type");
        classMap.MapProperty(b => b.MetaData).SetElementName("meta_data");
        classMap.MapProperty(b => b.CreatedAt).SetElementName("created_at");
        classMap.SetCollectionName(options.CollectionName);

        BsonClassMap.RegisterClassMap(classMap);

        return serviceCollection.AddScoped<IAuditMetaDataProvider, AuditMetaDataProvider>();
    }
}
