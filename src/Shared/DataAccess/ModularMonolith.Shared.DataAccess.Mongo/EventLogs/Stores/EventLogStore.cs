using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.DataAccess.EventLogs;
using ModularMonolith.Shared.DataAccess.Models;
using ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Factories;
using ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Models;
using ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Options;
using ModularMonolith.Shared.Events;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Stores;

internal sealed class EventLogStore : IEventLogStore
{
    private const int BatchSize = 1000;

    private readonly IMongoCollection<EventLogEntity> _mongoCollection;
    private readonly EventLogFactory _eventLogFactory;

    public EventLogStore(IMongoDatabase mongoDatabase, IOptions<EventOptions> optionsMonitor,
        EventLogFactory eventLogFactory)
    {
        _eventLogFactory = eventLogFactory;
        _mongoCollection = mongoDatabase.GetCollection<EventLogEntity>(optionsMonitor.Value.CollectionName);
    }

    public async Task<TEvent?> FindFirstOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventLog = await _mongoCollection.AsQueryable()
            .Where(e => e.MetaData.Subject == subject)
            .OrderBy(e => e.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : BsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public async Task<EventLogEntry?> FindFirstOccurenceAsync(string subject, Type eventType,
        CancellationToken cancellationToken)
    {
        var eventLog = await _mongoCollection.AsQueryable()
            .Where(e => e.MetaData.Subject == subject && e.EventTypeName == eventType.FullName)
            .OrderBy(e => e.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : _eventLogFactory.Create(eventLog);
    }

    public async Task<TEvent?> FindLastOccurenceAsync<TEvent>(string subject, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventLog = await _mongoCollection.AsQueryable()
            .Where(e => e.MetaData.Subject == subject)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : BsonSerializer.Deserialize<TEvent>(eventLog.EventPayload);
    }

    public async Task<EventLogEntry?> FindLastOccurenceAsync(string subject, Type eventType,
        CancellationToken cancellationToken)
    {
        var eventLog = await _mongoCollection.AsQueryable()
            .Where(e => e.MetaData.Subject == subject && e.EventTypeName == eventType.FullName)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        return eventLog is null ? default : _eventLogFactory.Create(eventLog);
    }

    public Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken)
    {
        var entity = _eventLogFactory.Create(entry);

        return _mongoCollection.InsertOneAsync(entity, new InsertOneOptions(), cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<EventLogEntry> entries, CancellationToken cancellationToken)
    {
        var entities = entries.Select(_eventLogFactory.Create);

        return _mongoCollection.InsertManyAsync(entities, new InsertManyOptions(), cancellationToken);
    }

    public async Task<DataPage<EventLogEntry>> FindAsync(Paginator paginator,
        EventLogSearchFilters filters,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilters(filters, _mongoCollection.AsQueryable());

        var orderedQuery = paginator.IsAscending
            ? query.OrderBy(e => e.Timestamp)
            : query.OrderByDescending(e => e.Timestamp);

        var dataTask = orderedQuery.Skip(paginator.Skip)
            .Take(paginator.Take)
            .ToListAsync(cancellationToken);

        var countTask = query.LongCountAsync(cancellationToken);

        await Task.WhenAll(dataTask, countTask);

        var items = await dataTask;
        var count = await countTask;

        var entries = items.Select(_eventLogFactory.Create).ToImmutableArray();

        return new DataPage<EventLogEntry>(entries, count);
    }

    private static IQueryable<EventLogEntity> ApplyFilters(EventLogSearchFilters filters,
        IQueryable<EventLogEntity> query)
    {
        if (filters.FromTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp >= filters.FromTimestamp.Value);
        }

        if (filters.ToTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp <= filters.ToTimestamp.Value);
        }

        if (filters.EventType is not null)
        {
            query = query.Where(e => e.EventTypeName == filters.EventType.FullName);
        }

        return query;
    }

    public async IAsyncEnumerable<EventLogEntry> FindAllAsync(EventLogSearchFilters filters,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = ApplyFilters(filters, _mongoCollection.AsQueryable());

        var batch = await query.Take(BatchSize).ToListAsync(cancellationToken);
        var skip = BatchSize;

        while (batch.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            batch = await query.Skip(skip).Take(BatchSize).ToListAsync(cancellationToken);

            foreach (var eventLogEntity in batch)
            {
                yield return _eventLogFactory.Create(eventLogEntity);
            }

            skip += BatchSize;
        }
    }
}
