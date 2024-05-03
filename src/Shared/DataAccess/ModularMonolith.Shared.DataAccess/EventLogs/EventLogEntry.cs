namespace ModularMonolith.Shared.DataAccess.EventLogs;

public sealed record EventLogEntry
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required Type EventType { get; init; }

    public required object EventInstance { get; init; }

    public required EventLogEntryMetaData MetaData { get; init; }
}
