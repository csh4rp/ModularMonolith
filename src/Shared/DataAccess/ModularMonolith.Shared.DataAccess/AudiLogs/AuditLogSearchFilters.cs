namespace ModularMonolith.Shared.DataAccess.AudiLogs;

public class AuditLogSearchFilters
{
    public DateTimeOffset? FromTimestamp { get; set; }

    public DateTimeOffset? ToTimestamp { get; set; }

    public Type? EntityType { get; set; }

    public string? Subject { get; set; }

    public bool Any() => FromTimestamp.HasValue
                         || ToTimestamp.HasValue
                         || EntityType is not null
                         || !string.IsNullOrEmpty(Subject);
}
