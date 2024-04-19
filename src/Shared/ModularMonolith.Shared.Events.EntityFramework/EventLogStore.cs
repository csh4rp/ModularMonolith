using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Events.EntityFramework;

internal class EventLogStore : IEventLogStore
{
    private readonly DbContext _dbContext;

    public EventLogStore(DbContext dbContext) => _dbContext = dbContext;

    public virtual async Task<TEvent?> FindFirstOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _dbContext.Set<EventLog>().Where(e => e.Subject == subject
                                                                   && e.EventType == eventType.FullName)
            .OrderBy(b => b.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : JsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public virtual Task<EventLog?> FindFirstOccurenceAsync(string subject, Type eventType, CancellationToken cancellationToken) =>
        _dbContext.Set<EventLog>().Where(e => e.Subject == subject
                                              && e.EventType == eventType.FullName)
            .OrderBy(b => b.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

    public virtual async Task<TEvent?> FindLastOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _dbContext.Set<EventLog>().Where(e => e.Subject == subject
                                                                   && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : JsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public virtual Task<EventLog?> FindLastOccurenceAsync(string subject, Type eventType, CancellationToken cancellationToken) =>
        _dbContext.Set<EventLog>().Where(e => e.Subject == subject
            && e.EventType == eventType.FullName)
        .OrderByDescending(b => b.OccurredAt)
        .FirstOrDefaultAsync(cancellationToken);
}
