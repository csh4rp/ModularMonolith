﻿using ModularMonolith.Shared.Domain.Abstractions;

namespace ModularMonolith.Shared.BusinessLogic.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken);
}
