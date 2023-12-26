using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.Utils;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class EventLogStore : IEventLogStore
{
    private readonly IEventLogDbContext _eventLogDbContext;
    private readonly EventSerializer _eventSerializer;

    public EventLogStore(IEventLogDbContext eventLogDbContext, EventSerializer eventSerializer)
    {
        _eventLogDbContext = eventLogDbContext;
        _eventSerializer = eventSerializer;
    }

    public async Task<TEvent?> GetFirstOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _eventLogDbContext.EventLogs.Where(e => e.UserId == userId
                                                                     && e.EventType == eventType.FullName)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : _eventSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public Task<EventLog?> GetFirstOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken) =>
        _eventLogDbContext.EventLogs
            .AsNoTracking()
            .Where(e => e.UserId == userId
                        && e.EventType == eventType.FullName)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TEvent?> GetLastOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _eventLogDbContext.EventLogs.Where(e => e.UserId == userId
                                                                     && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : _eventSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public Task<EventLog?> GetLastOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken) =>
        _eventLogDbContext.EventLogs
            .AsNoTracking()
            .Where(e => e.UserId == userId
                        && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
