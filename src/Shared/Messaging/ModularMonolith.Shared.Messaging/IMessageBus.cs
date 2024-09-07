using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging;

public interface IMessageBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class, IEvent;

    Task PublishAsync(IEnumerable<IEvent> @event, CancellationToken cancellationToken);

    Task SendAsync<TCommand>(ICommand command, CancellationToken cancellationToken) where TCommand : class, ICommand;
}
