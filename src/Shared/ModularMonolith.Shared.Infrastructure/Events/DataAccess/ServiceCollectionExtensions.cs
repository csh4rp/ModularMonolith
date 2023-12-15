using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Events;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventDataAccessServices<TEventContext>(this IServiceCollection serviceCollection)
         where TEventContext : DbContext, IEventLogContext =>
        serviceCollection.AddScoped<IEventBus, OutboxEventBus>()
            .AddScoped<IEventLogStore, EventLogStore>()
            .AddScoped<EventReader>()
            .AddScoped<IEventLogContext>(sp => sp.GetRequiredService<TEventContext>());
}
