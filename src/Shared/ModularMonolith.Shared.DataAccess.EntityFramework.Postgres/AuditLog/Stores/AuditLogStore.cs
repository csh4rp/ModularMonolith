using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.AudiLog;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLog.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLog.Models;
using ModularMonolith.Shared.DataAccess.Models;
using Z.EntityFramework.Plus;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLog.Stores;

internal sealed class AuditLogStore : IAuditLogStore
{
    private readonly DbContext _dbContext;
    private readonly AuditLogFactory _auditLogFactory;

    public AuditLogStore(DbContext dbContext, AuditLogFactory auditLogFactory)
    {
        _dbContext = dbContext;
        _auditLogFactory = auditLogFactory;
    }

    public Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = new())
    {
        var entity = _auditLogFactory.Create(entry);

        _dbContext.Set<AuditLogEntity>().Add(entity);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<AuditLogEntry> entries,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var entities = entries.Select(_auditLogFactory.Create);

        _dbContext.Set<AuditLogEntity>().AddRange(entities);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DataPage<AuditLogEntry>> FindAsync(Paginator<AuditLogEntry> paginator,
        AuditLogSearchFilters? filters = null,
        CancellationToken cancellationToken = new())
    {
        var query = filters is null
            ? _dbContext.Set<AuditLogEntity>().AsNoTracking()
            : FilterQuery(filters).AsNoTracking();

        var (orderByExpression, isAsc) = paginator.GetOrderByExpression();

        var orderedQueryable = isAsc ? query.OrderBy(MapOrderBy(orderByExpression)) : query.OrderByDescending(MapOrderBy(orderByExpression));

        var totalCountDeferred = query.DeferredLongCount();

        var items = await orderedQueryable.ToListAsync(cancellationToken);
        var totalCount = await totalCountDeferred.FutureValue().ValueAsync(cancellationToken);

        var entries = items.Select(_auditLogFactory.Create).ToImmutableArray();

        return new DataPage<AuditLogEntry>(entries, totalCount);
    }

    public async IAsyncEnumerable<AuditLogEntry> FindAllAsync(AuditLogSearchFilters? filters = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        const int take = 100;

        var query = filters is null
            ? _dbContext.Set<AuditLogEntity>().OrderBy(e => e.Timestamp).AsNoTracking()
            : FilterQuery(filters).OrderBy(e => e.Timestamp).AsNoTracking();

        var batch = await query.Take(take).ToListAsync(cancellationToken);
        var skip = take;

        while (batch.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            batch = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

            foreach (var auditLogEntity in batch)
            {
                yield return _auditLogFactory.Create(auditLogEntity);
            }

            skip += take;
        }
    }

    private IQueryable<AuditLogEntity> FilterQuery(AuditLogSearchFilters filters)
    {
        var query = _dbContext.Set<AuditLogEntity>().AsQueryable();

        if (filters.FromTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp >= filters.FromTimestamp);
        }

        if (filters.ToTimestamp.HasValue)
        {
            query = query.Where(e => e.Timestamp < filters.ToTimestamp);
        }

        if (filters.EntityType is not null)
        {
            query = query.Where(e => e.EntityTypeName == filters.EntityType.FullName);
        }

        return query;
    }

    private static Expression<Func<AuditLogEntity, object>> MapOrderBy(Expression<Func<AuditLogEntry, object>> expression)
    {

        return default!;
    }

}
