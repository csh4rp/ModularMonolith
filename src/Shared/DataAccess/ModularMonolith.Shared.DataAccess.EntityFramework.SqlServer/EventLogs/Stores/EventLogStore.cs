using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.Models;
using ModularMonolith.Shared.DataAccess.EventLogs;
using ModularMonolith.Shared.DataAccess.Models;
using Z.EntityFramework.Plus;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.Stores;

internal sealed class EventLogStore : IEventLogStore
{
    private const int BatchSize = 1000;

    private readonly DbContext _dbContext;
    private readonly EventLogFactory _eventLogFactory;

    public EventLogStore(DbContext dbContext, EventLogFactory eventLogFactory)
    {
        _dbContext = dbContext;
        _eventLogFactory = eventLogFactory;
    }

    public Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken)
    {
        var entity = _eventLogFactory.Create(entry);

        _dbContext.Set<EventLogEntity>().Add(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<EventLogEntry> entries,
        CancellationToken cancellationToken)
    {
        var entities = entries.Select(_eventLogFactory.Create);

        _dbContext.Set<EventLogEntity>().AddRange(entities);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DataPage<EventLogEntry>> FindAsync(Paginator paginator,
        EventLogSearchFilters filters,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilters(filters).AsNoTracking();

        var orderedQueryable = paginator.IsAscending
            ? query.OrderBy(e => e.Timestamp)
            : query.OrderByDescending(e => e.Timestamp);

        var totalCountDeferred = query.DeferredLongCount();

        var items = await orderedQueryable.ToListAsync(cancellationToken);
        var totalCount = await totalCountDeferred.FutureValue().ValueAsync(cancellationToken);

        var entries = items.Select(_eventLogFactory.Create).ToImmutableArray();

        return new DataPage<EventLogEntry>(entries, totalCount);
    }

    private IQueryable<EventLogEntity> ApplyFilters(EventLogSearchFilters filters)
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

    public async IAsyncEnumerable<EventLogEntry> FindAllAsync(EventLogSearchFilters filters,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var query = ApplyFilters(filters).OrderBy(e => e.Timestamp).AsNoTracking();

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
