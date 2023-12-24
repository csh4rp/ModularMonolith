using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Domain.Entities;

namespace ModularMonolith.Shared.Application.Events;

public interface IEventLogStore
{
    Task<TEvent?> GetFirstOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken)
        where TEvent : IEvent;

    Task<EventLog?> GetFirstOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken);

    Task<TEvent?> GetLastOccurenceAsync<TEvent>(Guid userId, CancellationToken cancellationToken) where TEvent : IEvent;

    Task<EventLog?> GetLastOccurenceAsync(Guid userId, Type eventType, CancellationToken cancellationToken);
}
