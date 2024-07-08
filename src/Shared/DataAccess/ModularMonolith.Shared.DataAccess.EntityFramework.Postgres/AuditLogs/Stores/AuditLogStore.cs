using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.Models;
using ModularMonolith.Shared.DataAccess.Models;
using Z.EntityFramework.Plus;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.Stores;

public class AuditLogStore : IAuditLogStore
{
    private const int BatchSize = 1000;

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
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<AuditLogEntry> entries,
        CancellationToken cancellationToken)
    {
        var entities = entries.Select(_auditLogFactory.Create);

        _dbContext.Set<AuditLogEntity>().AddRange(entities);
        return Task.CompletedTask;
    }

    public async Task<DataPage<AuditLogEntry>> FindAsync(Paginator paginator,
        AuditLogSearchFilters filters,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilters(filters).AsNoTracking();

        var orderedQueryable = paginator.IsAscending
            ? query.OrderBy(a => a.Timestamp)
            : query.OrderByDescending(a => a.Timestamp);

        var totalCountDeferred = query.DeferredLongCount();

        var items = await orderedQueryable.ToListAsync(cancellationToken);
        var totalCount = await totalCountDeferred.FutureValue().ValueAsync(cancellationToken);

        var entries = items.Select(_auditLogFactory.Create).ToImmutableArray();

        return new DataPage<AuditLogEntry>(entries, totalCount);
    }

    public async IAsyncEnumerable<AuditLogEntry> FindAllAsync(AuditLogSearchFilters filters,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var query = ApplyFilters(filters).AsNoTracking();

        var batch = await query.Take(BatchSize).ToListAsync(cancellationToken);
        var skip = BatchSize;

        while (batch.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            batch = await query.Skip(skip).Take(BatchSize).ToListAsync(cancellationToken);

            foreach (var auditLogEntity in batch)
            {
                yield return _auditLogFactory.Create(auditLogEntity);
            }

            skip += BatchSize;
        }
    }

    private IQueryable<AuditLogEntity> ApplyFilters(AuditLogSearchFilters filters)
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
}
