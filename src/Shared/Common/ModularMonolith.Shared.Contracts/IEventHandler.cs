using MediatR;
using ModularMonolith.Shared.Events;

namespace ModularMonolith.Shared.Contracts;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent;
