namespace ModularMonolith.Shared.DataAccess.EventLogs;

public sealed record EventLogEntry
{
    public required Guid Id { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required object Instance { get; init; }

    public required EventLogEntryMetaData MetaData { get; init; }
}
