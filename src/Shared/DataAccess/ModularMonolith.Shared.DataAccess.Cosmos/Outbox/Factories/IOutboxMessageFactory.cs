using ModularMonolith.Shared.DataAccess.Cosmos.Outbox.Models;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.DataAccess.Cosmos.Outbox.Factories;

public interface IOutboxMessageFactory
{
    OutboxMessage Create(IEvent @event);
}
