using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.AuditTrail.Mongo.Entity;
using ModularMonolith.Shared.AuditTrail.Mongo.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ModularMonolith.Shared.AuditTrail.Mongo;

public class AuditTrailBackgroundService : BackgroundService
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IOptionsMonitor<AuditTrailOptions> _optionsMonitor;

    public AuditTrailBackgroundService(IMongoDatabase mongoDatabase, IOptionsMonitor<AuditTrailOptions> optionsMonitor)
    {
        _mongoDatabase = mongoDatabase;
        _optionsMonitor = optionsMonitor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var collection = _mongoDatabase.GetCollection<AuditLogEntity>(_optionsMonitor.CurrentValue.CollectionName);

        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
            .Match(change =>
                change.OperationType == ChangeStreamOperationType.Insert
                || change.OperationType == ChangeStreamOperationType.Update
                || change.OperationType == ChangeStreamOperationType.Replace
                || change.OperationType == ChangeStreamOperationType.Delete);

        using var cursor = await _mongoDatabase.WatchAsync(pipeline, new ChangeStreamOptions { }, stoppingToken);

        while (await cursor.MoveNextAsync(stoppingToken))
        {
            foreach (var document in cursor.Current)
            {
                var state = document.OperationType switch
                {
                    ChangeStreamOperationType.Insert => EntityState.Added,
                    ChangeStreamOperationType.Replace or ChangeStreamOperationType.Update => EntityState.Modified,
                    ChangeStreamOperationType.Delete => EntityState.Deleted,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var log = new AuditLogEntity
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = document.ClusterTime.ToUniversalTime(),
                    EntityState = state,
                    EntityType = document.CollectionNamespace.CollectionName,
                    OperationName = document.
                }

                collection.
            }
        }
    }
}
