using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.Events.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection serviceCollection,
        params Assembly[] assemblies)
    {
        serviceCollection.AddScoped<IEventBus, EfEventBus>()
            .AddScoped<IEventLogStore, EfEventLogStore>();

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
