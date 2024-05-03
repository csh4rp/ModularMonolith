using ModularMonolith.Shared.DataAccess.Mongo.Outbox.Models;
using ModularMonolith.Shared.Events;
using MongoDB.Bson;

namespace ModularMonolith.Shared.DataAccess.Mongo.Outbox.Factories;

internal sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    public OutboxMessage Create(IEvent @event) => new()
    {
        Id = @event.EventId,
        Timestamp = @event.Timestamp,
        MessagePayload = BsonDocument.Create(@event),
        MessageTypeName = @event.GetType().FullName!
    };
}
