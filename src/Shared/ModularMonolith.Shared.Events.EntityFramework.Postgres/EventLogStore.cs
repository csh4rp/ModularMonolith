using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Events.EntityFramework.Postgres.Entities;

namespace ModularMonolith.Shared.Events.EntityFramework.Postgres;

internal class EventLogStore : IEventLogStore
{
    private readonly DbContext _dbContext;

    public EventLogStore(DbContext dbContext) => _dbContext = dbContext;

    public virtual async Task<TEvent?> FindFirstOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _dbContext.Set<EventLogEntity>().Where(e => e.Subject == subject
                                                                   && e.EventType == eventType.FullName)
            .OrderBy(b => b.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : eventLog.EventPayload.Deserialize<TEvent>();
    }

    public virtual async Task<EventLog?> FindFirstOccurenceAsync(string subject, Type eventType,
        CancellationToken cancellationToken)
    {
        var eventLog = await _dbContext.Set<EventLogEntity>().Where(e => e.Subject == subject
                                                                         && e.EventType == eventType.FullName)
            .OrderBy(b => b.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null
            ? default
            : new EventLog
            {
                Id = eventLog.Id,
                OccurredAt = eventLog.OccurredAt,
                EventType = eventType,
                EventPayload = eventLog.EventPayload.Deserialize(eventType)!,
                Subject = eventLog.Subject
            };
    }

    public virtual async Task<TEvent?> FindLastOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _dbContext.Set<EventLogEntity>().Where(e => e.Subject == subject
                                                                         && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : eventLog.EventPayload.Deserialize<TEvent>();
    }

    public virtual async Task<EventLog?> FindLastOccurenceAsync(string subject, Type eventType,
        CancellationToken cancellationToken)
    {
        var eventLog = await _dbContext.Set<EventLogEntity>()
            .Where(e => e.Subject == subject
                                                    && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null
            ? default
            : new EventLog
            {
                Id = eventLog.Id,
                OccurredAt = eventLog.OccurredAt,
                EventType = eventType,
                EventPayload = eventLog.EventPayload.Deserialize(eventType)!,
                Subject = eventLog.Subject
            };
    }


    public async Task<List<EventLog>> GetAllAsync(DateTimeOffset from, CancellationToken cancellationToken)
    {
        var eventLogs = await _dbContext.Set<EventLogEntity>()
            .Where(e => e.OccurredAt <= from)
            .ToListAsync(cancellationToken);

        return eventLogs.Select(e =>
        {
            var type = Type.GetType(e.EventType)!;

            return new EventLog
            {
                Id = e.Id,
                OccurredAt = e.OccurredAt,
                EventType = type,
                EventPayload = e.EventPayload.Deserialize(type)!,
                Subject = e.Subject
            };
        }).ToList();
    }
}
