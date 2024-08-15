namespace ModularMonolith.Shared.DataAccess.EventLogs;

public sealed record EventLogSearchFilters
{
    public static readonly EventLogSearchFilters Empty = new();

    public DateTimeOffset? FromTimestamp { get; init; }

    public DateTimeOffset? ToTimestamp { get; init; }

    public Type? EventType { get; init; }
}
