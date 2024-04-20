using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Events.Mongo.Entities;
using ModularMonolith.Shared.Events.Mongo.Options;
using MongoDB.Bson.Serialization;

namespace ModularMonolith.Shared.Events.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection serviceCollection,
        Action<EventOptions> options,
        params Assembly[] assemblies)
    {
        serviceCollection.AddScoped<IEventBus, EventBus>()
            .AddScoped<IEventLogStore, EventLogStore>();

        BsonClassMap.RegisterClassMap<EventLogEntity>(classMap =>
        {
            classMap.MapIdMember(b => b.Id);
            classMap.MapMember(b => b.Subject).SetElementName("subject");
            classMap.MapMember(b => b.CorrelationId).SetElementName("correlation_id");
            classMap.MapMember(b => b.EventType).SetElementName("event_type");
            classMap.MapMember(b => b.OccurredAt).SetElementName("occurred_at");
            classMap.MapExtraElementsField(b => b.EventPayload).SetElementName("event_payload");
        });

        foreach (var assembly in assemblies)
        {
            var mappings = assembly.GetExportedTypes()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableTo(typeof(IEventMapping<>)));

            foreach (var mapping in mappings)
            {
                var interfaces = mapping.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    serviceCollection.Add(new ServiceDescriptor(@interface, mapping, ServiceLifetime.Scoped));
                }
            }
        }

        return serviceCollection;
    }
}
