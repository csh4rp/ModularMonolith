using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Events;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Repositories;

public abstract class CrudRepository<TAggregate, TId> where TAggregate : AggregateRoot<TId> where TId : IEquatable<TId>
{
    protected IMongoDatabase Database { get; set; }

    protected IEventBus EventBus { get; set; }

    protected virtual IMongoCollection<TAggregate> Collection => Database.GetCollection<TAggregate>(typeof(TAggregate).Name);

    protected CrudRepository(IMongoDatabase database, IEventBus eventBus)
    {
        Database = database;
        EventBus = eventBus;
    }

    public async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        await Collection.InsertOneAsync(aggregate, new InsertOneOptions(), cancellationToken);

        var events = aggregate.DequeueEvents();
        await EventBus.PublishAsync(events, cancellationToken);
    }

    public async Task AddAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        var aggregatesList = aggregates as IReadOnlyCollection<TAggregate> ?? aggregates.ToList();
        await Collection.InsertManyAsync(aggregatesList, new InsertManyOptions{Comment = ""}, cancellationToken);

        var events = aggregatesList.SelectMany(a => a.DequeueEvents());
        await EventBus.PublishAsync(events, cancellationToken);
    }

    public async Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var result = await Collection.ReplaceOneAsync(a => a.Id.Equals(aggregate.Id)
                                                           && a.Version == aggregate.Version - 1,
            aggregate,
            new ReplaceOptions
            {
                IsUpsert = false
            },
            cancellationToken: cancellationToken);

        var events = aggregate.DequeueEvents();
        await EventBus.PublishAsync(events, cancellationToken);

        if (result.ModifiedCount == 0)
        {
            throw new InvalidOperationException("Could not match aggregate version to replace");
        }
    }

    public Task UpdateAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        var tasks = aggregates.Select(a => UpdateAsync(a, cancellationToken)).ToArray();

        return Task.WhenAll(tasks);
    }

    public async Task RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var result = await Collection.DeleteOneAsync(a => a.Id.Equals(aggregate.Id), cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new InvalidOperationException("Could not match aggregate to replace");
        }
    }

    public async Task RemoveAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        var ids = aggregates.Select(a => a.Id);

        var result = await Collection.DeleteManyAsync(a => ids.Contains(a.Id), cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new InvalidOperationException("Could not match aggregate to replace");
        }
    }

    public async Task<TAggregate?> FindByIdAsync(TId id, CancellationToken cancellationToken)
    {
        using var cursor = await Collection.FindAsync(a => a.Id.Equals(id),
            new FindOptions<TAggregate>{Limit = 1},
            cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            return cursor.Current.FirstOrDefault();
        }

        return null;
    }

    public async Task<List<TAggregate>> FindAllByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken)
    {
        var items = new List<TAggregate>();

        using var cursor = await Collection.FindAsync(a => ids.Contains(a.Id),
            new FindOptions<TAggregate>{Limit = int.MaxValue},
            cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            items.AddRange(cursor.Current);
        }

        return items;
    }

}
