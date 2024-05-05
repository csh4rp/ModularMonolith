using Microsoft.Azure.Cosmos;
using ModularMonolith.Shared.DataAccess.Cosmos.Outbox.Factories;
using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Repositories;

public abstract class CrudRepository<TAggregate, TId> where TAggregate : AggregateRoot<TId> where TId : IEquatable<TId>
{
    public CosmosClient _cosmosClient;

    public IOutboxMessageFactory OutboxMessageFactory { get; }

    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var container = _cosmosClient.GetContainer("", "");

        var events = aggregate.DequeueEvents();


        var batch = container.CreateTransactionalBatch(new PartitionKey(""));

        batch.CreateItem(aggregate, new TransactionalBatchItemRequestOptions());

        foreach (var domainEvent in events)
        {
            var outboxMessage = OutboxMessageFactory.Create(domainEvent);

            batch.CreateItem(outboxMessage, new TransactionalBatchItemRequestOptions());
        }

        using var response = await batch.ExecuteAsync(new TransactionalBatchRequestOptions(), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {

        }
    }
}
