using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.AuditTrail.Mongo.Entity;
using MongoDB.Bson.Serialization;

namespace ModularMonolith.Shared.AuditTrail.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add(this IServiceCollection serviceCollection)
    {
        BsonClassMap.RegisterClassMap<AuditLogEntity>(classMap =>
        {
            classMap.MapIdMember(b => b.Id);
            classMap.MapMember(b => b.Subject).SetElementName("subject");
        });

        return serviceCollection;
    }
}
