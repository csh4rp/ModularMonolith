using MediatR;

namespace ModularMonolith.Shared.Domain.Abstractions;

public interface IEventHandler<in T> : INotificationHandler<T> where T : IEvent;
