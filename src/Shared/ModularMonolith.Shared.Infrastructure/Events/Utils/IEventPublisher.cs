using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Infrastructure.Events.Utils;

internal interface IEventPublisher
{
    Task PublishAsync(EventLog eventLog, CancellationToken cancellationToken);
}
