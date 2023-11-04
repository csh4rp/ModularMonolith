using ModularMonolith.Shared.BusinessLogic;

namespace ModularMonolith.Shared.Infrastructure.Events;

internal sealed class EventBus : IEventBus
{
    public Task PublishAsync(object @event, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task PublishAsync(IEnumerable<object> events, CancellationToken cancellationToken) => throw new NotImplementedException();
}
