using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;


namespace ModularMonolith.Shared.Infrastructure.Events;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection serviceCollection,
        params Assembly[] assemblies)
    {
        serviceCollection.AddScoped<IEventBus, OutboxEventBus>()
            .AddScoped<IEventLogStore, EventLogStore>()
            .AddScoped<IEventLogDatabase>(sp =>
            {
                var context = sp.GetRequiredService<DbContext>();
                return new EventLogDatabase(context);
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
