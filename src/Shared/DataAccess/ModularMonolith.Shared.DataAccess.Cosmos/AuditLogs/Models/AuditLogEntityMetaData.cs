﻿namespace ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Models;

public record AuditLogEntityMetaData
{
    public required string? Subject { get; init; }
    
    public required string? OperationName { get; init; }

    public required string? TraceId { get; init; }

    public required string? SpanId { get; init; }

    public required string? ParentSpanId { get; init; }

    public required Dictionary<string, string?> ExtraData { get; init; }
}
