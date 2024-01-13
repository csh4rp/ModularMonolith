using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class EventLogStore : IEventLogStore
{
    private readonly IEventLogDatabase _database;

    public EventLogStore(IEventLogDatabase database) => _database = database;

    public async Task<TEvent?> GetFirstOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _database.EventLogs.Where(e => e.UserId == userId
                                                                     && e.EventType == eventType.FullName)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : JsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public Task<EventLog?> GetFirstOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken) =>
        _database.EventLogs
            .AsNoTracking()
            .Where(e => e.UserId == userId
                        && e.EventType == eventType.FullName)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TEvent?> GetLastOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _database.EventLogs.Where(e => e.UserId == userId
                                                                     && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : JsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public Task<EventLog?> GetLastOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken) =>
        _database.EventLogs
            .AsNoTracking()
            .Where(e => e.UserId == userId
                        && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
