namespace ModularMonolith.Shared.Domain.Entities;

public class EventLogPublishAttempt
{
    public Guid EventLogId { get; init; }

    public int AttemptNumber { get; init; }

    public DateTimeOffset NextAttemptAt { get; init; }
}
