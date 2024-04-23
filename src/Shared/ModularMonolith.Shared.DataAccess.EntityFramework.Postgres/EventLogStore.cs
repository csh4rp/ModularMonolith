using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Entities;
using ModularMonolith.Shared.DataAccess.EventLog;
using ModularMonolith.Shared.DataAccess.Models;
using Z.EntityFramework.Plus;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres;

internal sealed class EventLogStore : IEventLogStore
{
    private readonly DbContext _dbContext;

    public EventLogStore(DbContext dbContext) => _dbContext = dbContext;

    public Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken = new())
    {
        var entity = Map(entry);

        _dbContext.Set<EventLogEntity>().Add(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<EventLogEntry> entries,
        CancellationToken cancellationToken = new())
    {
        var entities = entries.Select(Map);

        _dbContext.Set<EventLogEntity>().AddRange(entities);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DataPage<EventLogEntry>> FindAsync(Paginator<EventLogEntry> paginator,
        EventLogSearchFilters? filters = null,
        CancellationToken cancellationToken = new())
    {
        var query = filters is null
            ? _dbContext.Set<EventLogEntity>().AsNoTracking()
            : FilterQuery(filters).AsNoTracking();

        var (orderByExpression, isAsc) = paginator.GetOrderByExpression();

        var orderedQueryable = isAsc ? query.OrderBy(MapOrderBy(orderByExpression)) : query.OrderByDescending(MapOrderBy(orderByExpression));

        var totalCountDeferred = query.DeferredLongCount();

        var items = await orderedQueryable.ToListAsync(cancellationToken);
        var totalCount = await totalCountDeferred.FutureValue().ValueAsync(cancellationToken);

        var entries = items.Select(Map).ToImmutableArray();

        return new DataPage<EventLogEntry>(entries, totalCount);
    }

    private IQueryable<EventLogEntity> FilterQuery(EventLogSearchFilters filters)
    {
        var query = _dbContext.Set<EventLogEntity>().AsQueryable();

        if (filters.FromTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp >= filters.FromTimestamp);
        }

        if (filters.ToTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp < filters.ToTimestamp);
        }

        if (filters.EventType is not null)
        {
            query = query.Where(e => e.EventTypeName == filters.EventType.FullName);
        }

        return query;
    }

    public async IAsyncEnumerable<EventLogEntry> FindAllAsync(EventLogSearchFilters? filters = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        const int take = 100;

        var query = filters is null
                ? _dbContext.Set<EventLogEntity>().OrderBy(e => e.Timestamp).AsNoTracking()
                : FilterQuery(filters).OrderBy(e => e.Timestamp).AsNoTracking();

        var batch = await query.Take(take).ToListAsync(cancellationToken);
        var skip = take;

        while (batch.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            batch = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

            foreach (var eventLogEntity in batch)
            {
                yield return Map(eventLogEntity);
            }

            skip += take;
        }
    }

    private static EventLogEntry Map(EventLogEntity entity)
    {
        var type = Type.GetType(entity.EventTypeName)!;

        return new EventLogEntry
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp,
            EventInstance = entity.EventPayload.Deserialize(type)!,
            EventType = type,
            MetaData = new EventLogEntryMetaData
            {
                Subject = entity.MetaData.Subject,
                Uri = entity.MetaData.Uri is null ? null : new Uri(entity.MetaData.Uri),
                IpAddress = entity.MetaData.IpAddress is null ? null : IPAddress.Parse(entity.MetaData.IpAddress),
                OperationName = entity.MetaData.OperationName,
                TraceId = entity.MetaData.TraceId,
                SpanId = entity.MetaData.SpanId,
                ParentSpanId = entity.MetaData.ParentSpanId
            }
        };
    }

    private static EventLogEntity Map(EventLogEntry entry) =>
        new()
        {
            Id = entry.Id,
            Timestamp = entry.Timestamp,
            EventPayload = JsonSerializer.SerializeToDocument(entry.EventInstance),
            EventTypeName = entry.EventType.FullName!,
            MetaData = new EventLogEntityMetaData
            {
                Subject = entry.MetaData.Subject,
                Uri = entry.MetaData.Uri?.ToString(),
                IpAddress = entry.MetaData.IpAddress?.ToString(),
                OperationName = entry.MetaData.OperationName,
                TraceId = entry.MetaData.TraceId,
                SpanId = entry.MetaData.SpanId,
                ParentSpanId = entry.MetaData.ParentSpanId
            }
        };

    private Expression<Func<EventLogEntity, object>> MapOrderBy(Expression<Func<EventLogEntry, object>> expression)
    {
        return default!;
    }
}
