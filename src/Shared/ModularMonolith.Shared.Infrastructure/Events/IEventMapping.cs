using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.Infrastructure.Events;

public interface IEventMapping<in TEvent> where TEvent : IEvent
{
    IIntegrationEvent Map(TEvent @event);
}
