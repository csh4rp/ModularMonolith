﻿using Microsoft.Extensions.Options;
using ModularMonolith.Shared.AuditTrail.Mongo.Model;
using ModularMonolith.Shared.AuditTrail.Mongo.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
        var auditLogCollection = _mongoDatabase.GetCollection<AuditLogEntity>(_optionsMonitor.CurrentValue.CollectionName);

        var loggedCollectionNames = BsonClassMap.GetRegisteredClassMaps()
            .Where(m => m.UsesAuditLog())
            .Select(m => m.GetCollectionName())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
            .Match(change =>
                loggedCollectionNames.Contains(change.CollectionNamespace.CollectionName)
                &&
                (
                    change.OperationType == ChangeStreamOperationType.Insert
                    || change.OperationType == ChangeStreamOperationType.Update
                    || change.OperationType == ChangeStreamOperationType.Replace)
                );

        using var cursor = await _mongoDatabase.WatchAsync(pipeline, new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.WhenAvailable,
            BatchSize = 100,
            StartAtOperationTime = new BsonTimestamp(DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds())
        }, stoppingToken);

        while (await cursor.MoveNextAsync(stoppingToken))
        {
            foreach (var document in cursor.Current)
            {
                var state = document.OperationType switch
                {
                    ChangeStreamOperationType.Insert => EntityState.Added,
                    ChangeStreamOperationType.Replace or ChangeStreamOperationType.Update => EntityState.Modified,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (!document.BackingDocument.TryGetElement("__audit", out var meteData))
                {
                    continue;
                }

                var changes = GetChanges(document);
                if (!changes.Any())
                {
                    continue;
                }

                var classMap = BsonClassMap.GetRegisteredClassMaps()
                    .First(c =>
                        c.GetCollectionName() == document.CollectionNamespace.CollectionName);

                var collection =
                    _mongoDatabase.GetCollection<BsonDocument>(document.CollectionNamespace.CollectionName);

                var auditLog = new AuditLogEntity
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = document.ClusterTime.ToUniversalTime(),
                    EntityState = state,
                    EntityKeys = document.DocumentKey,
                    EntityType = classMap.ClassType.FullName!,
                    EntityPropertyChanges = changes,
                    MetaData = BsonSerializer.Deserialize<AuditMetaData>(meteData.ToString()),
                };

                var versionMemberMap = classMap.GetMemberMap("Version");
                var version = document.FullDocument["version"]?.AsInt32;

                var idFilter = Builders<BsonDocument>.Filter.Eq(classMap.IdMemberMap.ElementName, document.DocumentKey.Values);
                var versionFilter = Builders<BsonDocument>.Filter.Eq(versionMemberMap.ElementName, version);
                var filter = Builders<BsonDocument>.Filter.And(idFilter, versionFilter);
                var update = Builders<BsonDocument>.Update.Unset(f => f["__audit"]);

                await auditLogCollection.InsertOneAsync(auditLog, new InsertOneOptions(), stoppingToken);

                // If version does not match, just skip it, probably another update already happened
                _ = await collection.UpdateOneAsync(filter, update, new UpdateOptions(), stoppingToken);
            }
        }
    }

    private static BsonDocument GetChanges(ChangeStreamDocument<BsonDocument> document)
    {
        var changes = new BsonDocument();

        foreach (var element in document.FullDocument)
        {
            if (!document.FullDocumentBeforeChange.TryGetElement(element.Name, out var prevElement))
            {
                prevElement = new BsonElement(element.Name, null);
            }

            var elementChange = new BsonDocument
            {
                { "currentValue", element.Value },
                { "originalValue", prevElement.Value }
            };

            changes[element.Name] = elementChange;
        }

        foreach (var element in document.FullDocumentBeforeChange)
        {
            if (!document.FullDocument.TryGetElement(element.Name, out var currentElement))
            {
                currentElement = new BsonElement(element.Name, null);
            }

            var elementChange = new BsonDocument
            {
                { "currentValue", currentElement.Value },
                { "originalValue", element.Value }
            };

            changes[element.Name] = elementChange;
        }

        return changes;
    }
}
