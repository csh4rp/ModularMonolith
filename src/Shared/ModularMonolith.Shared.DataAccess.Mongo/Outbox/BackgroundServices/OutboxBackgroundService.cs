using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Attributes;
using ModularMonolith.Shared.DataAccess.Mongo.Outbox.Models;
using ModularMonolith.Shared.DataAccess.Mongo.Outbox.Options;
using ModularMonolith.Shared.Events;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Outbox.BackgroundServices;

public class OutboxBackgroundService : BackgroundService
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpoint _sendEndpoint;
    private readonly TimeProvider _timeProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<OutboxOptions> _optionsMonitor;

    public OutboxBackgroundService(IMongoDatabase mongoDatabase,
        IPublishEndpoint publishEndpoint,
        TimeProvider timeProvider,
        IServiceProvider serviceProvider,
        IOptionsMonitor<OutboxOptions> optionsMonitor, ISendEndpoint sendEndpoint)
    {
        _mongoDatabase = mongoDatabase;
        _publishEndpoint = publishEndpoint;
        _timeProvider = timeProvider;
        _serviceProvider = serviceProvider;
        _optionsMonitor = optionsMonitor;
        _sendEndpoint = sendEndpoint;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _optionsMonitor.CurrentValue;
        var collectionName = options.CollectionName;

        var startTime = _timeProvider.GetUtcNow().Add(-1 * options.ChangeDataCaptureDiff);
        var startOffset = startTime - DateTimeOffset.UnixEpoch;
        var startTimestamp = new BsonTimestamp((long)startOffset.TotalSeconds);

        var collection = _mongoDatabase.GetCollection<OutboxMessage>(collectionName);
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<OutboxMessage>>()
            .Match(change => change.OperationType == ChangeStreamOperationType.Insert);

        using var cursor = await collection.WatchAsync(pipeline,
            new ChangeStreamOptions
        {
            StartAtOperationTime = startTimestamp,
            BatchSize = options.ChangeDataCaptureBatchSize,
        }, stoppingToken);

        var documentIdsToDelete = new List<Guid>(options.ChangeDataCaptureBatchSize);

        while (await cursor.MoveNextAsync(stoppingToken))
        {
            foreach (var document in cursor.Current)
            {
                var messageType = Type.GetType(document.FullDocument.MessageTypeName)!;
                var messageInstance = BsonSerializer.Deserialize(document.BackingDocument, messageType);

                var eventAttribute = messageType.GetCustomAttribute<EventAttribute>();
                var commandAttribute = messageType.GetCustomAttribute<CommandAttribute>();

                if (eventAttribute is not null)
                {
                    await _publishEndpoint.Publish(messageInstance, messageType, context =>
                    {
                        context.MessageId = document.FullDocument.Id;
                    }, stoppingToken);

                    var mappingType = typeof(IEventMapping<>).MakeGenericType(messageType);
                    var mapping = _serviceProvider.GetService(mappingType);

                    if (mapping is not null)
                    {
                        var method = mappingType.GetMethod(nameof(IEventMapping<IEvent>.Map))!;
                        var integrationEvent = (IntegrationEvent)method.Invoke(mapping, [messageInstance])!;

                        await _publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), context =>
                        {
                            context.InitiatorId = document.FullDocument.Id;
                        }, stoppingToken);
                    }
                }
                else if (commandAttribute is not null)
                {
                    await _sendEndpoint.Send(messageInstance, messageType, context =>
                    {
                        context.MessageId = document.FullDocument.Id;
                    }, stoppingToken);
                }

                documentIdsToDelete.Add(document.FullDocument.Id);
            }

            await collection.DeleteManyAsync(f => documentIdsToDelete.Contains(f.Id), stoppingToken);
            documentIdsToDelete.Clear();
        }
    }
}
