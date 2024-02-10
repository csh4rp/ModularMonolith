using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Events;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;

internal sealed class EventLogStore : IEventLogStore
{
    private readonly DbContext _dbContext;

    public EventLogStore(DbContext dbContext) => _dbContext = dbContext;

    public async Task<TEvent?> GetFirstOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _dbContext.Set<EventLog>().Where(e => e.UserId == userId
                                                                     && e.EventType == eventType.FullName)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : JsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public Task<EventLog?> GetFirstOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken) =>
        _dbContext.Set<EventLog>()
            .AsNoTracking()
            .Where(e => e.UserId == userId
                        && e.EventType == eventType.FullName)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TEvent?> GetLastOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var eventLog = await _dbContext.Set<EventLog>().Where(e => e.UserId == userId
                                                                   && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : JsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public Task<EventLog?> GetLastOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken) =>
        _dbContext.Set<EventLog>()
            .AsNoTracking()
            .Where(e => e.UserId == userId
                        && e.EventType == eventType.FullName)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
