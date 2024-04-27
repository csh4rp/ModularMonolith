using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLog.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLog.Models;
using ModularMonolith.Shared.DataAccess.EventLog;
using ModularMonolith.Shared.DataAccess.Models;
using Z.EntityFramework.Plus;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLog.Stores;

internal sealed class EventLogStore : IEventLogStore
{
    private readonly DbContext _dbContext;
    private readonly EventLogFactory _eventLogFactory;

    public EventLogStore(DbContext dbContext, EventLogFactory eventLogFactory)
    {
        _dbContext = dbContext;
        _eventLogFactory = eventLogFactory;
    }

    public Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken = new())
    {
        var entity = _eventLogFactory.Create(entry);


        _dbContext.Set<EventLogEntity>().Add(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<EventLogEntry> entries,
        CancellationToken cancellationToken = new())
    {
        var entities = entries.Select(_eventLogFactory.Create);

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

        var entries = items.Select(_eventLogFactory.Create).ToImmutableArray();

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
                yield return _eventLogFactory.Create(eventLogEntity);
            }

            skip += take;
        }
    }



    private Expression<Func<EventLogEntity, object>> MapOrderBy(Expression<Func<EventLogEntry, object>> expression)
    {
        return default!;
    }
}
