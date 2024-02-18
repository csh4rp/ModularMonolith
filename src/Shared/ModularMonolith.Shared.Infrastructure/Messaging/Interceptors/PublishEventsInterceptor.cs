using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.Infrastructure.Messaging.Interceptors;

public class PublishEventsInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var bus = eventData.Context!.GetService<IEventBus>();
        var events = GetEvents(eventData);

        if (events.Count > 0)
        {
            bus.PublishAsync(events, CancellationToken.None).GetAwaiter().GetResult();
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        var bus = eventData.Context!.GetService<IEventBus>();
        var events = GetEvents(eventData);

        if (events.Count > 0)
        {
            await bus.PublishAsync(events, CancellationToken.None);
        }

        return result;
    }

    private static List<IEvent> GetEvents(DbContextEventData eventData) =>
        eventData.Context!.ChangeTracker.Entries()
            .Where(e => e.Entity is IEntity)
            .Select(e => (IEntity)e.Entity)
            .SelectMany(e => e.DequeueEvents()).ToList();
}
