using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.EventLogs.Interceptors;

public class EventLogInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        Debug.Assert(eventData.Context is not null);

        var events = eventData.Context.ChangeTracker.Entries()
            .Where(e => e.Entity is IEntity)
            .SelectMany(e => ((IEntity)e.Entity).DequeueEvents())
            .ToList();

        if (events.Count > 0)
        {
            var bus = eventData.Context.GetService<IMessageBus>();
            await bus.PublishAsync(events, cancellationToken);
        }

        return result;
    }

    public override InterceptionResult<int>
        SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) =>
        SavingChangesAsync(eventData, result).GetAwaiter().GetResult();
}
