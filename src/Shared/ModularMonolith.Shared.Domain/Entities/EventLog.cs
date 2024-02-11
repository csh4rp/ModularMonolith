namespace ModularMonolith.Shared.Domain.Entities;

public sealed class EventLog
{
    public required Guid Id { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public Guid? UserId { get; init; }

    public required string EventType { get; init; }

    public required string EventPayload { get; init; }
}
