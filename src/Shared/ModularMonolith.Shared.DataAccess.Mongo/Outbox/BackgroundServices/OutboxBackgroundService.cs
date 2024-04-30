using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Options;
using ModularMonolith.Shared.DataAccess.Mongo.Outbox.Models;
using ModularMonolith.Shared.Events;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Outbox.BackgroundServices;

public class OutboxBackgroundService : BackgroundService
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TimeProvider _timeProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<EventOptions> _optionsMonitor;

    public OutboxBackgroundService(IMongoDatabase mongoDatabase,
        IPublishEndpoint publishEndpoint,
        TimeProvider timeProvider,
        IServiceProvider serviceProvider,
        IOptionsMonitor<EventOptions> optionsMonitor)
    {
        _mongoDatabase = mongoDatabase;
        _publishEndpoint = publishEndpoint;
        _timeProvider = timeProvider;
        _serviceProvider = serviceProvider;
        _optionsMonitor = optionsMonitor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var collectionName = _optionsMonitor.CurrentValue.CollectionName;

        var startTime = _timeProvider.GetUtcNow().AddHours(-1);
        var startOffset = startTime - DateTimeOffset.UnixEpoch;
        var startTimestamp = new BsonTimestamp((long)startOffset.TotalSeconds);

        var collection = _mongoDatabase.GetCollection<OutboxMessage>(collectionName);
        using var cursor = await collection.WatchAsync(new ChangeStreamOptions
        {
            StartAtOperationTime = startTimestamp
        }, stoppingToken);

        while (await cursor.MoveNextAsync(stoppingToken))
        {
            foreach (var document in cursor.Current)
            {
                if (document.OperationType != ChangeStreamOperationType.Insert)
                {
                    continue;
                }

                var messageType = Type.GetType(document.FullDocument.MessageTypeName)!;
                var messageInstance = BsonSerializer.Deserialize(document.BackingDocument, messageType);
                var eventAttribute = messageType.GetCustomAttribute<EventAttribute>()!;


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

                if (!eventAttribute.IsPersisted)
                {
                    await collection.DeleteOneAsync(f => f.Id == document.FullDocument.Id, new DeleteOptions(), stoppingToken);
                }
            }
        }
    }
}
