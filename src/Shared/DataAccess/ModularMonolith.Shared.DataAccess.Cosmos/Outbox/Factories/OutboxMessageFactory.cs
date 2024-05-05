using ModularMonolith.Shared.DataAccess.Cosmos.Outbox.Models;
using ModularMonolith.Shared.Events;
using Newtonsoft.Json.Linq;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Outbox.Factories;

public class OutboxMessageFactory : IOutboxMessageFactory
{
    public OutboxMessage Create(IEvent @event) => new()
    {
        Id = @event.EventId,
        Timestamp = @event.Timestamp,
        MessagePayload = JObject.FromObject(@event),
        MessageTypeName = @event.GetType().FullName!
    };
}
