using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Messaging;

public interface IMessageBus
{
    Task PublishAsync(IEvent @event, CancellationToken cancellationToken);

    Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken);

    Task SendAsync(ICommand command, CancellationToken cancellationToken);
}
