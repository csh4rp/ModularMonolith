namespace ModularMonolith.Shared.Domain.Entities;

public sealed class EventLog
{
    public Guid Id { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public required Guid? UserId { get; init; }
    
    public required string? UserName { get; init; }

    public required Guid? CorrelationId { get; init; }

    public required string EventType { get; init; }

    public required string EventName { get; init; }

    public required string EventPayload { get; init; }

    public required string OperationName { get; init; }

    public required string TraceId { get; init; }
    
    public required string SpanId { get; init; }

    public required string? ParentSpanId { get; init; }
    
    public required string? IpAddress { get; init; }

    public required string? UserAgent { get; init; }
}
