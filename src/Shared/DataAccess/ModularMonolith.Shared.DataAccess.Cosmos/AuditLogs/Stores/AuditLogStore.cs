using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Factories;
using ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Models;
using ModularMonolith.Shared.DataAccess.Cosmos.Options;
using ModularMonolith.Shared.DataAccess.Models;

namespace ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Stores;

public class AuditLogStore : IAuditLogStore
{
    private readonly CosmosClient _cosmosClient;
    private readonly AuditLogFactory _auditLogFactory;
    private readonly IOptions<CosmosOptions> _options;

    public AuditLogStore(CosmosClient cosmosClient, AuditLogFactory auditLogFactory, IOptions<CosmosOptions> options)
    {
        _cosmosClient = cosmosClient;
        _auditLogFactory = auditLogFactory;
        _options = options;
    }

    public async Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken)
    {
        var container = GetContainer();
        var entity = _auditLogFactory.Create(entry);

        var response = await container.CreateItemAsync(entity, new PartitionKey(entity.PartitionKey), new ItemRequestOptions(),
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            // Throw
        }
    }


    public async Task AddRangeAsync(IEnumerable<AuditLogEntry> entries, CancellationToken cancellationToken)
    {
        var container = GetContainer();
        var entities = entries.Select(_auditLogFactory.Create);

        var groups = entities.GroupBy(e => e.PartitionKey);

        var batches = groups.Select(async group =>
        {
            var batch = container.CreateTransactionalBatch(new PartitionKey(group.Key));
            foreach (var auditLogEntity in group)
            {
                batch.CreateItem(auditLogEntity, new TransactionalBatchItemRequestOptions());
            }

            var batchResponse = await batch.ExecuteAsync(cancellationToken);

            if (!batchResponse.IsSuccessStatusCode)
            {
// Throw
            }
        }).ToArray();

        await Task.WhenAll(batches);
    }

    public async Task<DataPage<AuditLogEntry>> FindAsync(Paginator paginator,
        AuditLogSearchFilters filters,
        CancellationToken cancellationToken = new())
    {
        var container = GetContainer();
        var (whereClause, parameters) = BuildWhereClause(filters);

        var selectClause = new StringBuilder($"SELECT * FROM c {whereClause}");
        var selectClauseParameters = new Dictionary<string, object>(parameters);
        var countClauseParameters = new Dictionary<string, object>(parameters);

        selectClause.Append(paginator.IsAscending
            ? " ORDER BY c.timestamp ASC OFFSET @offset LIMIT @limit"
            : " ORDER BY c.timestamp DESC OFFSET @offset LIMIT @limit");

        selectClauseParameters.Add("@offset", paginator.Skip);
        selectClauseParameters.Add("@limit", paginator.Take);

        var selectQuery = new QueryDefinition(selectClause.ToString());
        var countQuery = new QueryDefinition($"SELECT VALUE COUNT(1) FROM c {whereClause}");

        foreach (var parameter in selectClauseParameters)
        {
            selectQuery = selectQuery.WithParameter(parameter.Key, parameter.Value);
        }

        foreach (var parameter in countClauseParameters)
        {
            countQuery = countQuery.WithParameter(parameter.Key, parameter.Value);
        }

        using var selectIterator = container.GetItemQueryIterator<AuditLogEntity>(selectQuery, null, new QueryRequestOptions
        {
            PartitionKey = filters.EntityType is null ? null : new PartitionKey(filters.EntityType.Name)
        });

        var result = new List<AuditLogEntity>();
        while (selectIterator.HasMoreResults)
        {
            foreach (var entity in await selectIterator.ReadNextAsync(cancellationToken))
            {
                result.Add(entity);
            }
        }

        using var countIterator = container.GetItemQueryIterator<long>(countQuery, null,
            new QueryRequestOptions
            {
                PartitionKey = filters.EntityType is null ? null : new PartitionKey(filters.EntityType.Name)
            });

        var count = await countIterator.ReadNextAsync(cancellationToken);

        return new DataPage<AuditLogEntry>([..result.Select(_auditLogFactory.Create)], count.First());
    }

    private static (string WhereClause, Dictionary<string, object> Parameters) BuildWhereClause(
        AuditLogSearchFilters filters)
    {
        var parameters = new Dictionary<string, object>();
        var whereClause = new StringBuilder("WHERE");

        if (filters.FromTimestamp.HasValue)
        {
            if (parameters.Count > 0)
            {
                whereClause.Append(" AND ");
            }

            whereClause.Append("c.timestamp >= @fromTimestamp");
            parameters.Add("@fromTimestamp", filters.FromTimestamp);
        }

        if (filters.ToTimestamp.HasValue)
        {
            if (parameters.Count > 0)
            {
                whereClause.Append(" AND ");
            }

            whereClause.Append("c.timestamp <= @toTimestamp");
            parameters.Add("@toTimestamp", filters.ToTimestamp);
        }

        if (filters.EntityType is not null)
        {
            if (parameters.Count > 0)
            {
                whereClause.Append(" AND ");
            }

            whereClause.Append("c.partitionKey = @partitionKey AND c.entityType = @entityType");
            parameters.Add("@partitionKey", filters.EntityType.Name);
            parameters.Add("@entityType", filters.EntityType.FullName!);
        }

        if (!string.IsNullOrEmpty(filters.Subject))
        {
            if (parameters.Count > 0)
            {
                whereClause.Append(" AND ");
            }

            whereClause.Append("c.subject = @subject");
            parameters.Add("@subject", filters.Subject);
        }

        return (whereClause.ToString(), parameters);
    }

    public async IAsyncEnumerable<AuditLogEntry> FindAllAsync(AuditLogSearchFilters filters,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var container = GetContainer();
        var (whereClause, parameters) = BuildWhereClause(filters);

        var selectQuery = new QueryDefinition($"SELECT * FROM c {whereClause}");

        foreach (var parameter in parameters)
        {
            selectQuery = selectQuery.WithParameter(parameter.Key, parameter.Value);
        }

        using var selectIterator = container.GetItemQueryIterator<AuditLogEntity>(selectQuery, null, new QueryRequestOptions
        {
            PartitionKey = filters.EntityType is null ? null : new PartitionKey(filters.EntityType.Name)
        });

        while (selectIterator.HasMoreResults)
        {
            foreach (var entity in await selectIterator.ReadNextAsync(cancellationToken))
            {
                yield return _auditLogFactory.Create(entity);
            }
        }
    }

    private Container GetContainer() => _cosmosClient.GetContainer(_options.Value.DatabaseId, _options.Value.AuditLogOptions.ContainerName);
}
