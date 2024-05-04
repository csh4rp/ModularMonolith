namespace ModularMonolith.Shared.DataAccess.EntityFramework.Cosmos.EventLogs.Models;

public class EventLogEntity
{
    public required Guid Id { get; init; }

    public required string PartitionKey { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required string EventTypeName { get; init; }

    public required string EventPayload { get; init; }

    public required EventLogEntityMetaData MetaData { get; init; }
}
