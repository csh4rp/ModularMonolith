using System.Dynamic;
using System.Net;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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

    public CrudRepository(CosmosClient cosmosClient, IOutboxMessageFactory outboxMessageFactory, IAuditMetaDataProvider auditMetaDataProvider, IOptions<CosmosOptions> options)
    {
        CosmosClient = cosmosClient;
        OutboxMessageFactory = outboxMessageFactory;
        AuditMetaDataProvider = auditMetaDataProvider;
        Options = options;
        Mmapping = EntityMapping.Get<TAggregate>();
    }

    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var container = GetContainer();
        var events = aggregate.DequeueEvents();

        var document = new CosmosDocument(aggregate);

        if (Mmapping.IsAuditEnabled)
        {
            document.SetAuditMetaData(AuditMetaDataProvider.GetMetaData());
        }

        var batch = container.CreateTransactionalBatch(Mmapping.GetPartitionKey(aggregate))
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

    public virtual async Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var container = GetContainer();
        var events = aggregate.DequeueEvents();

        var document = new CosmosDocument(aggregate);

        if (Mmapping.IsAuditEnabled)
        {
            document.SetAuditMetaData(AuditMetaDataProvider.GetMetaData());
        }

        var batch = container.CreateTransactionalBatch(Mmapping.GetPartitionKey(aggregate))
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
        var container = GetContainer();

        var response = await container.DeleteItemAsync<TAggregate>(aggregate.Id.ToString(), Mmapping.GetPartitionKey(aggregate),
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

    public virtual Task<List<TAggregate>> FindAllByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken)
    {
        var container = GetContainer();

        var queryBuilder = new StringBuilder("SELECT * FROM c WHERE c.id IN (");
        var parameters = new Dictionary<string, object>();

        var index = 0;
        foreach (var id in ids)
        {
            var parameterName = $"@id{index++}";

            parameters.Add(parameterName, id);
            queryBuilder.Append(parameterName)
        }


        using var iterator =
            container.GetItemQueryIterator<TAggregate>(query, null, new QueryRequestOptions());

        if (!iterator.HasMoreResults)
        {
            return null;
        }

        var response = await iterator.ReadNextAsync(cancellationToken);
        using var enumerator = response.GetEnumerator();
        return enumerator.Current;
    }
}
