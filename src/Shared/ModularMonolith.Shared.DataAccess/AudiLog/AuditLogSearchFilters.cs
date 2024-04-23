﻿namespace ModularMonolith.Shared.DataAccess.AudiLog;

public class AuditLogSearchFilters
{
    public DateTimeOffset? FromTimestamp { get; set; }

    public DateTimeOffset? ToTimestamp { get; set; }

    public Type? EntityType { get; set; }

    public string? Subject { get; set; }
}
