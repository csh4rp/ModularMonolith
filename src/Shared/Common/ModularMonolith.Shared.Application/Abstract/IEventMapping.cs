using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Application.Abstract;

public interface IEventMapping<in TEvent> where TEvent : IEvent
{
    IntegrationEvent Map(TEvent @event);
}
