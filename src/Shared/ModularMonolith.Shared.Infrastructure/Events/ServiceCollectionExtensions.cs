using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.Options;

namespace ModularMonolith.Shared.Infrastructure.Events;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection serviceCollection,
        Action<EventOptions> action)
    {
        serviceCollection.AddOptions<EventOptions>()
            .Configure(o =>
            {
                o.PollInterval = TimeSpan.FromSeconds(30);
                o.MaxLockTime = TimeSpan.FromSeconds(30);
                o.TimeBetweenAttempts = TimeSpan.FromSeconds(30);
                o.MaxParallelWorkers = Environment.ProcessorCount * 2;
                o.MaxRetryAttempts = 10;
                o.MaxPollBatchSize = 20;
                o.MaxEventChannelSize = 100;
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
