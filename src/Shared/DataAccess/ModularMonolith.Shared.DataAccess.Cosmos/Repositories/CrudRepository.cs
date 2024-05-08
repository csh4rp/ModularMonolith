using System.Net;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.Cosmos.Common;
using ModularMonolith.Shared.DataAccess.Cosmos.Mappings;
using ModularMonolith.Shared.DataAccess.Cosmos.Options;
using ModularMonolith.Shared.DataAccess.Cosmos.Outbox.Factories;
using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Repositories;

public abstract class CrudRepository<TAggregate, TId> where TAggregate : AggregateRoot<TId> where TId : IEquatable<TId>
{
    protected EntityMapping<TAggregate> Mmapping { get; }

    protected CosmosClient CosmosClient;

    protected IOutboxMessageFactory OutboxMessageFactory { get; }

    protected IAuditMetaDataProvider AuditMetaDataProvider { get; }

    protected IOptions<CosmosOptions> Options { get; }

    protected Container Container { get; }

    public CrudRepository(CosmosClient cosmosClient, IOutboxMessageFactory outboxMessageFactory, IAuditMetaDataProvider auditMetaDataProvider, IOptions<CosmosOptions> options)
    {
        CosmosClient = cosmosClient;
        OutboxMessageFactory = outboxMessageFactory;
        AuditMetaDataProvider = auditMetaDataProvider;
        Options = options;
        Mmapping = EntityMapping.Get<TAggregate>();
        Container = CosmosClient.GetContainer(Options.Value.DatabaseId, Mmapping.Container);
    }

    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.DequeueEvents();

        var document = new CosmosDocument(aggregate);

        if (Mmapping.IsAuditEnabled)
        {
            document.SetAuditMetaData(AuditMetaDataProvider.GetMetaData());
        }

        var batch = Container.CreateTransactionalBatch(Mmapping.GetPartitionKey(aggregate))
            .CreateItem(aggregate, new TransactionalBatchItemRequestOptions());

        foreach (var domainEvent in events)
        {
            var outboxMessage = OutboxMessageFactory.Create(domainEvent);
            var outboxMessageDocument = new CosmosDocument(outboxMessage);

            batch = batch.CreateItem(outboxMessageDocument, new TransactionalBatchItemRequestOptions());
        }

        using var response = await batch.ExecuteAsync(new TransactionalBatchRequestOptions(), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {

        }
    }

    public virtual async Task AddRangeAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = default;
        var documents = new List<CosmosDocument>();

        foreach (var aggregate in aggregates)
        {
            var currentPartitionKey = Mmapping.GetPartitionKey(aggregate);

            if (partitionKey != default && partitionKey != currentPartitionKey)
            {
                throw new InvalidOperationException("Can't add multiple entities with different PartitionKeys");
            }

            var document = new CosmosDocument(aggregate);
            if (Mmapping.IsAuditEnabled)
            {
                document.SetAuditMetaData(AuditMetaDataProvider.GetMetaData());
            }

            foreach (var domainEvent in aggregate.DequeueEvents())
            {
                var outboxMessage = OutboxMessageFactory.Create(domainEvent);
                var outboxMessageDocument = new CosmosDocument(outboxMessage);
                documents.Add(outboxMessageDocument);
            }

            documents.Add(document);
        }

        var batch = Container.CreateTransactionalBatch(partitionKey);

        foreach (var cosmosDocument in documents)
        {
            batch = batch.CreateItem(cosmosDocument);
        }

        using var response = await batch.ExecuteAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {

        }
    }

    public virtual async Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.DequeueEvents();

        var document = new CosmosDocument(aggregate);

        if (Mmapping.IsAuditEnabled)
        {
            document.SetAuditMetaData(AuditMetaDataProvider.GetMetaData());
        }

        var batch = Container.CreateTransactionalBatch(Mmapping.GetPartitionKey(aggregate))
            .UpsertItem(aggregate, new TransactionalBatchItemRequestOptions
            {
                IfMatchEtag = aggregate.Version.ToString()
            });

        foreach (var domainEvent in events)
        {
            var outboxMessage = OutboxMessageFactory.Create(domainEvent);
            var outboxMessageDocument = new CosmosDocument(outboxMessage);

            batch = batch.CreateItem(outboxMessageDocument, new TransactionalBatchItemRequestOptions
            {

            });
        }

        using var response = await batch.ExecuteAsync(new TransactionalBatchRequestOptions(), cancellationToken);
    }

    public virtual async Task RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var response = await Container.DeleteItemAsync<TAggregate>(aggregate.Id.ToString(), Mmapping.GetPartitionKey(aggregate),
            new ItemRequestOptions(), cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {

        }
    }

    public virtual async Task<TAggregate?> FindByIdAsync(TId id, CancellationToken cancellationToken)
    {
        var container = GetContainer();

        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        using var iterator =
            container.GetItemQueryIterator<TAggregate>(query, null, new QueryRequestOptions { MaxItemCount = 1 });

        if (!iterator.HasMoreResults)
        {
            return null;
        }

        var response = await iterator.ReadNextAsync(cancellationToken);
        using var enumerator = response.GetEnumerator();
        return enumerator.Current;
    }

    private Container GetContainer() => CosmosClient.GetContainer(Options.Value.DatabaseId, Mmapping.Container);

    public virtual async Task<List<TAggregate>> FindAllByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken)
    {
        var idsList = ids as IReadOnlyList<TId> ?? ids.ToList();

        var queryBuilder = new StringBuilder("SELECT * FROM c WHERE c.id IN (");
        var parameters = new Dictionary<string, object>();

        for (var i = 0; i < idsList.Count; i++)
        {
            var parameterName = $"@id{i}";
            parameters.Add(parameterName, idsList[i]);
            queryBuilder.Append(parameterName);
            queryBuilder.Append(i < idsList.Count - 1 ? ',' : ')');
        }

        var queryDefinition = new QueryDefinition(queryBuilder.ToString());

        foreach (var (key, value) in parameters)
        {
            queryDefinition = queryDefinition.WithParameter(key, value);
        }

        using var iterator = Container.GetItemQueryIterator<TAggregate>(queryDefinition, null, new QueryRequestOptions());

        var items = new List<TAggregate>();

        while (iterator.HasMoreResults)
        {
            items.AddRange(await iterator.ReadNextAsync(cancellationToken));
        }

        return items;
    }

    public virtual async Task<bool> ExistsByIdAsync(TId id, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition("SELECT id FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        using var iterator =
            Container.GetItemQueryIterator<TAggregate>(query, null, new QueryRequestOptions { MaxItemCount = 1 });

        if (!iterator.HasMoreResults)
        {
            return false;
        }

        var response = await iterator.ReadNextAsync(cancellationToken);
        using var enumerator = response.GetEnumerator();
        return enumerator.Current is not null;
    }
}
