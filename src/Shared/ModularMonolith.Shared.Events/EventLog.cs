namespace ModularMonolith.Shared.Events;

public sealed class EventLog
{
    public required Guid Id { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public string? Subject { get; init; }

    public required string EventType { get; init; }

    public required string EventPayload { get; init; }
}
