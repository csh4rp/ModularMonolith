using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;

namespace ModularMonolith.Shared.Infrastructure.Events.Extensions;

public static class EventExtensions
{
    public static IServiceCollection AddEvents<TContext>(this IServiceCollection serviceCollection, Action<EventOptions> configureAction) 
        where TContext : class, IEventLogContext
    {
        serviceCollection.AddScoped<IEventLogContext, TContext>(sp => sp.GetRequiredService<TContext>())
            .AddScoped<IEventBus, OutboxEventBus>()
            .AddScoped<EventPublisherBackgroundService>()
            .AddSingleton<EventMapper>()
            .AddSingleton<EventSerializer>();

        serviceCollection.AddOptions<EventOptions>()
            .Configure(configureAction);

        return serviceCollection;
    }
    
    
}
