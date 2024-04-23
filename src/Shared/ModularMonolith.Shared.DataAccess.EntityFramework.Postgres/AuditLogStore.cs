using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.AudiLog;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.Entities;
using ModularMonolith.Shared.DataAccess.Models;
using Z.EntityFramework.Plus;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres;

public class AuditLogStore : IAuditLogStore
{
    public required DbContext _dbContext;

    public Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = new())
    {
        var entity = new AuditLogEntity
        {
            Id = entry.Id,
            Timestamp = entry.Timestamp,
            EntityKey = JsonSerializer.SerializeToDocument(entry.EntityKey),
            EntityChanges = JsonSerializer.SerializeToDocument(entry.EntityChanges),
            EntityTypeName = entry.EntityType.FullName!,
            OperationType = entry.OperationType,
            MetaData = new AuditLogEntityMetaData
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

        _dbContext.Set<AuditLogEntity>().Add(entity);
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

        var entries = items.Select(Map).ToImmutableArray();

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

            foreach (var eventLogEntity in batch)
            {
                yield return Map(eventLogEntity);
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

    private static AuditLogEntry Map(AuditLogEntity entity)
    {
        var type = Type.GetType(entity.EntityTypeName)!;

        return new AuditLogEntry
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp,
            EntityType = type,
            EntityChanges = entity.EntityChanges.Deserialize<EntityChanges>()!,
            EntityKey = entity.EntityKey.Deserialize<EntityKey>()!,
            OperationType = entity.OperationType,
            MetaData = new AuditMetaData
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

    private Expression<Func<AuditLogEntity, object>> MapOrderBy(Expression<Func<AuditLogEntry, object>> expression)
    {
        return default!;
    }

}
