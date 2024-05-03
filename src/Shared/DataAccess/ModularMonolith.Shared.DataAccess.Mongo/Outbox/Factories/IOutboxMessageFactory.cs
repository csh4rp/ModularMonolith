using ModularMonolith.Shared.DataAccess.Mongo.Outbox.Models;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.DataAccess.Mongo.Outbox.Factories;

public interface IOutboxMessageFactory
{
    OutboxMessage Create(IEvent @event);
}
