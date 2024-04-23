namespace ModularMonolith.Shared.DataAccess.EventLog;

public class EventLogSearchFilters
{
    public DateTimeOffset? FromTimestamp { get; set; }

    public DateTimeOffset? ToTimestamp { get; set; }

    public Type? EventType { get; set; }
}
