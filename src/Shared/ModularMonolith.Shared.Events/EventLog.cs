namespace ModularMonolith.Shared.Events;

public sealed record EventLog
{
    public required Guid Id { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public string? Subject { get; init; }

    public required Type EventType { get; init; }

    public required object EventPayload { get; init; }
}
