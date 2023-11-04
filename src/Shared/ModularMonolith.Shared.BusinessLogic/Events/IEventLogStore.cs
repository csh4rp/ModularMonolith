namespace ModularMonolith.Shared.BusinessLogic.Events;

public interface IEventLogStore
{
    Task<TEvent?> GetLastOccurenceAsync<TEvent>(string userId, CancellationToken cancellationToken);
}
