using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Events;
using ModularMonolith.Shared.Infrast1ructure.Events.DataAccess.Concrete;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventDataAccessServices(this IServiceCollection serviceCollection) =>
        serviceCollection.AddScoped<IEventBus, OutboxEventBus>()
            .AddScoped<IEventLogStore, EventLogStore>()
            .AddScoped<EventReader>();
}
