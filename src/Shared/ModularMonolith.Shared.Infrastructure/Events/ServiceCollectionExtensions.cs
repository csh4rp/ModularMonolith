using Microsoft.Extensions.DependencyInjection;
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
                o.MaxParallelWorkers = Environment.ProcessorCount * 2;
                o.MaxRetryAttempts = 10;
                o.MaxLockTime = TimeSpan.FromSeconds(30);
            })
            .PostConfigure(action);

        return serviceCollection;
    }
}
