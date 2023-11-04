namespace ModularMonolith.Shared.BusinessLogic;

public interface IEventBus
{
    Task PublishAsync(object @event, CancellationToken cancellationToken);

    Task PublishAsync(IEnumerable<object> events, CancellationToken cancellationToken);
}
