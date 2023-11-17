using MediatR;

namespace ModularMonolith.Shared.BusinessLogic.Events;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : INotification;
