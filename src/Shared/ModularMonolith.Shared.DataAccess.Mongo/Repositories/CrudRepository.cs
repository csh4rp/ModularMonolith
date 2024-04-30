using Microsoft.Extensions.Options;
using ModularMonolith.Shared.AuditTrail;
using ModularMonolith.Shared.AuditTrail.Mongo;
using ModularMonolith.Shared.AuditTrail.Mongo.Model;
using ModularMonolith.Shared.AuditTrail.Mongo.Options;
using ModularMonolith.Shared.DataAccess.Mongo.Outbox.Models;
using ModularMonolith.Shared.DataAccess.Mongo.Outbox.Options;
using ModularMonolith.Shared.DataAccess.Mongo.Transactions;
using ModularMonolith.Shared.Domain.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Repositories;

public abstract class CrudRepository<TAggregate, TId> where TAggregate : AggregateRoot<TId> where TId : IEquatable<TId>
{
    private readonly BsonClassMap _classMap;

    protected CrudRepository(IOptions<AuditTrailOptions> auditOptions,
        IOptions<OutboxOptions> outboxOutboxOptions,
        IMongoDatabase database,
        IAuditMetaDataProvider auditMetaDataProvider,
        TimeProvider timeProvider)
    {
        _classMap = BsonClassMap.LookupClassMap(typeof(TAggregate));
        AuditOptions = auditOptions;
        OutboxOptions = outboxOutboxOptions;
        Database = database;
        AuditMetaDataProvider = auditMetaDataProvider;
        TimeProvider = timeProvider;
    }

    protected IOptions<AuditTrailOptions> AuditOptions { get; }

    protected IOptions<OutboxOptions> OutboxOptions { get; }


    protected IMongoDatabase Database { get; }

    protected IAuditMetaDataProvider AuditMetaDataProvider { get; }

    protected TimeProvider TimeProvider { get; }

    protected virtual IMongoCollection<BsonDocument> Collection => Database.GetCollection<BsonDocument>(_classMap.GetCollectionName());

    protected virtual IMongoCollection<OutboxMessage> OutboxMessageCollection => Database.GetCollection<OutboxMessage>(OutboxOptions.Value.CollectionName);

    public Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var document = CreateDocument(aggregate);
        var events = aggregate.DequeueEvents();

        var outboxMessages = CreateOutboxMessages(events);
        if (outboxMessages.Count == 0)
        {
            return Collection.InsertOneAsync(document, new InsertOneOptions(), cancellationToken);
        }

        return RunInTransactionAsync(async ct =>
        {
            await Collection.InsertOneAsync(document, new InsertOneOptions(), cancellationToken);
            await OutboxMessageCollection.InsertManyAsync(outboxMessages, new InsertManyOptions(), ct);
        }, cancellationToken);
    }

    private static List<OutboxMessage> CreateOutboxMessages(IEnumerable<DomainEvent> events)
    {
        var outboxMessages = events.Select(e => new OutboxMessage
        {
            Id = e.EventId,
            Timestamp = e.Timestamp,
            MessagePayload = BsonDocument.Create(e),
            MessageTypeName = e.GetType().FullName!,
        }).ToList();
        return outboxMessages;
    }

    private async Task RunInTransactionAsync(Func<CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        IClientSessionHandle? session = null;

        try
        {
            if (UnitOfWorkScope.Current.Value is null)
            {
                session = await Database.Client.StartSessionAsync(new ClientSessionOptions(), cancellationToken);
                session.StartTransaction();
            }

            await func(cancellationToken);

            if (session?.IsInTransaction is true)
            {
                await session.CommitTransactionAsync(cancellationToken);
            }
        }
        finally
        {
            session?.Dispose();
        }
    }

    public Task AddAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        var aggregatesList = aggregates as IReadOnlyCollection<TAggregate> ?? aggregates.ToList();
        var documents = aggregatesList.Select(CreateDocument).ToList();
        var events = aggregatesList.SelectMany(a => a.DequeueEvents());

        var outboxMessages = CreateOutboxMessages(events);
        if (outboxMessages.Count == 0)
        {
            return Collection.InsertManyAsync(documents, new InsertManyOptions(), cancellationToken);
        }

        return RunInTransactionAsync(async ct =>
        {
            await Collection.InsertManyAsync(documents, new InsertManyOptions(), cancellationToken);
            await OutboxMessageCollection.InsertManyAsync(outboxMessages, new InsertManyOptions(), ct);
        }, cancellationToken);
    }

    public Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var document = CreateDocument(aggregate);
        var events = aggregate.DequeueEvents();
        var outboxMessages = CreateOutboxMessages(events);

        var idMemberMap = _classMap.GetMemberMap(nameof(aggregate.Id));
        var versionMemberMap = _classMap.GetMemberMap(nameof(aggregate.Version));

        var idFilter = Builders<BsonDocument>.Filter.Eq(idMemberMap.ElementName, aggregate.Id);
        var versionFilter = Builders<BsonDocument>.Filter.Eq(versionMemberMap.ElementName, aggregate.Version);
        var filter = Builders<BsonDocument>.Filter.And(idFilter, versionFilter);

        if (outboxMessages.Count == 0)
        {
            return Update(cancellationToken);
        }

        return RunInTransactionAsync(async ct =>
        {
            await Update(ct);
            await OutboxMessageCollection.InsertManyAsync(outboxMessages, new InsertManyOptions(), ct);
        }, cancellationToken);

        async Task Update(CancellationToken ct)
        {
            var result = await Collection.ReplaceOneAsync(filter,
                document,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken: ct);

            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException("Could not match aggregate version to replace");
            }
        }
    }

    public Task UpdateAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        var versionMemberMap = _classMap.GetMemberMap(nameof(AggregateRoot<int>.Version));

        return RunInTransactionAsync(async ct =>
        {
            var messages = new List<OutboxMessage>();

            foreach (var aggregate in aggregates)
            {
                var idFilter = Builders<BsonDocument>.Filter.Eq(_classMap.IdMemberMap.ElementName, aggregate.Id);
                var versionFilter = Builders<BsonDocument>.Filter.Eq(versionMemberMap.ElementName, aggregate.Version);
                var filter = Builders<BsonDocument>.Filter.And(idFilter, versionFilter);

                var document = CreateDocument(aggregate);

                var result = await Collection.ReplaceOneAsync(filter,
                    document,
                    new ReplaceOptions { IsUpsert = false },
                    cancellationToken: ct);

                if (result.ModifiedCount == 0)
                {
                    throw new InvalidOperationException("Could not match aggregate version to replace");
                }

                messages.AddRange(CreateOutboxMessages(aggregate.DequeueEvents()));
            }

            await OutboxMessageCollection.InsertManyAsync(messages, new InsertManyOptions(), ct);

        }, cancellationToken);
    }

    public async Task RemoveAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        var idFilter = Builders<BsonDocument>.Filter.Eq(_classMap.IdMemberMap.ElementName, aggregate.Id);
        var auditLogCollection = Database.GetCollection<AuditLogEntity>(AuditOptions.Value.CollectionName);

        await RunInTransactionAsync(async ct =>
        {
            var result = await Collection.DeleteOneAsync(idFilter, ct);
            var auditLog = CreateDeletedAuditLog(aggregate, _classMap.IdMemberMap);

            await auditLogCollection.InsertOneAsync(auditLog, new InsertOneOptions(), ct);

            if (result.DeletedCount == 0)
            {
                throw new InvalidOperationException("Could not match aggregate to replace");
            }
        }, cancellationToken);
    }

    private AuditLogEntity CreateDeletedAuditLog(TAggregate aggregate, BsonMemberMap idMemberMap) =>
        new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = TimeProvider.GetUtcNow().UtcDateTime,
            EntityState = EntityState.Deleted,
            EntityType = typeof(TAggregate).FullName!,
            EntityKeys = new BsonDocument { { idMemberMap.ElementName, idMemberMap.Getter(aggregate).ToString() } },
            EntityPropertyChanges = [],
            MetaData = AuditMetaDataProvider.MetaData,
        };

    public async Task RemoveAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken)
    {
        var aggregatesList = aggregates as IReadOnlyCollection<TAggregate>  ?? aggregates.ToList();
        var ids = aggregatesList.Select(a => a.Id);

        var idFilter = Builders<BsonDocument>.Filter.In(_classMap.IdMemberMap.ElementName, ids);
        var logs = aggregatesList.Select(a => CreateDeletedAuditLog(a, _classMap.IdMemberMap));
        var auditLogCollection = Database.GetCollection<AuditLogEntity>(AuditOptions.Value.CollectionName);

        await RunInTransactionAsync(async ct =>
        {
            var result = await Collection.DeleteManyAsync(idFilter, ct);
            await auditLogCollection.InsertManyAsync(logs, new InsertManyOptions(), cancellationToken);

            if (result.DeletedCount == 0)
            {
                throw new InvalidOperationException("Could not match aggregate to replace");
            }
        }, cancellationToken);
    }

    public async Task<TAggregate?> FindByIdAsync(TId id, CancellationToken cancellationToken)
    {
        var idFilter = Builders<TAggregate>.Filter.Eq(_classMap.IdMemberMap.ElementName, id);
        var collection = Database.GetCollection<TAggregate>(typeof(TAggregate).Name);

        return await collection.Find(idFilter).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<TAggregate>> FindAllByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken)
    {
        var idFilter = Builders<TAggregate>.Filter.In(_classMap.IdMemberMap.ElementName, ids);
        var collection = Database.GetCollection<TAggregate>(typeof(TAggregate).Name);

        return collection.Find(idFilter).ToListAsync(cancellationToken);
    }

    private BsonDocument CreateDocument(TAggregate aggregate)
    {
        var document = aggregate.ToBsonDocument();
        var metaData = AuditMetaDataProvider.MetaData;

        document["__audit"] = metaData.ToBsonDocument();
        document["__type"] = typeof(TAggregate).FullName;
        return document;
    }

}
