using MediatR;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Events.Events;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

public class EventHandler : INotificationHandler<DomainEvent>
{
    public Task Handle(DomainEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
