namespace ModularMonolith.Shared.Domain.Entities;

public sealed class EventLog
{
    public Guid Id { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }
    
    public Guid? UserId { get; init; }

    public required Guid? CorrelationId { get; init; }

    public required string Type { get; init; }

    public required string Name { get; init; }

    public required string Payload { get; init; }

    public required string OperationName { get; init; }

    public required string ActivityId { get; init; }
    
    public required string? IpAddress { get; init; }
    
    public required string? UserAgent { get; init; }
}
