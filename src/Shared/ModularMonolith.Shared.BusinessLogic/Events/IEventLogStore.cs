namespace ModularMonolith.Shared.BusinessLogic.Events;

public interface IEventLogStore
{
    Task<TEvent?> GetFirstOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken);
    
    Task<TEvent?> GetLastOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken);
    
}
