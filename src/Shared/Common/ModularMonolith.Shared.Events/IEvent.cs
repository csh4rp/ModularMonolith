using MediatR;

namespace ModularMonolith.Shared.Events;

public interface IEvent : INotification
{
    Guid EventId { get; }

    DateTimeOffset Timestamp { get; }
}
