using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;

namespace ModularMonolith.Shared.Infrastructure.Events;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection serviceCollection,
        Action<EventOptions> action)
    {
        serviceCollection.AddEventBackgroundServices()
            .AddEventDataAccessServices()
            .AddEventMetaDataProvider()
            .AddEventUtils();

        serviceCollection.AddOptions<EventOptions>()
            .Configure(o =>
            {
                o.PollInterval = TimeSpan.FromSeconds(30);
                o.MaxLockTime = TimeSpan.FromSeconds(30);
                o.TimeBetweenAttempts = TimeSpan.FromSeconds(30);
                o.MaxParallelWorkers = Environment.ProcessorCount * 2;
                o.MaxRetryAttempts = 10;
                o.MaxPollBatchSize = 20;
                o.RunBackgroundWorkers = true;
            })
            .PostConfigure(action)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = new EventOptions();
        action(options);

        var assembliesToScan = options.Assemblies;

        foreach (var assembly in assembliesToScan)
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
