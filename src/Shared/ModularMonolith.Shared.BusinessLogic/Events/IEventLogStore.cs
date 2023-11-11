namespace ModularMonolith.Shared.BusinessLogic.Events;

public interface IEventLogStore
{
    Task<TEvent?> GetLastOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken);
}
