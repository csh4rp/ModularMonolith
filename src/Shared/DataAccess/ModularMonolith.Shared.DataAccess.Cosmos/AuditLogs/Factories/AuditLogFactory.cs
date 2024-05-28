﻿using System.Diagnostics;
using System.Net;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Models;

namespace ModularMonolith.Shared.DataAccess.Cosmos.AuditLogs.Factories;

public class AuditLogFactory
{
    public AuditLogEntity Create(AuditLogEntry entry) => new()
    {
        Id = entry.Id,
        PartitionKey = entry.EntityType.Name,
        Timestamp = entry.Timestamp,
        EntityKey = entry.EntityKey.Select(k => new EntityField(k.Name, k.Value)).ToArray(),
        EntityChanges =
            entry.EntityChanges.Select(k => new EntityFieldChange(k.Name, k.OriginalValue, k.CurrentValue))
                .ToArray(),
        EntityTypeName = entry.EntityType.FullName!,
        OperationType = entry.OperationType,
        MetaData = new AuditLogEntityMetaData
        {
            Subject = entry.MetaData.Subject,
            Uri = entry.MetaData.Uri?.ToString(),
            IpAddress = entry.MetaData.IpAddress?.ToString(),
            OperationName = entry.MetaData.OperationName,
            TraceId = entry.MetaData.TraceId?.ToString(),
            SpanId = entry.MetaData.SpanId?.ToString(),
            ParentSpanId = entry.MetaData.ParentSpanId?.ToString(),
            ExtraData = []
        }
    };

    public AuditLogEntry Create(AuditLogEntity entity) => new()
    {
        Id = entity.Id,
        Timestamp = entity.Timestamp,
        EntityType = Type.GetType(entity.EntityTypeName)!,
        EntityKey = new EntityKey(entity.EntityKey),
        EntityChanges = new EntityChanges(entity.EntityChanges),
        OperationType = entity.OperationType,
        MetaData = new AuditMetaData
        {
            Subject = entity.MetaData.Subject,
            Uri = entity.MetaData.Uri is null ? null : new Uri(entity.MetaData.Uri),
            IpAddress = entity.MetaData.IpAddress is null ? null : IPAddress.Parse(entity.MetaData.IpAddress),
            OperationName = entity.MetaData.OperationName,
            TraceId =
                entity.MetaData.TraceId is null
                    ? null
                    : ActivityTraceId.CreateFromString(entity.MetaData.TraceId),
            SpanId =
                entity.MetaData.SpanId is null ? null : ActivitySpanId.CreateFromString(entity.MetaData.SpanId),
            ParentSpanId =
                entity.MetaData.ParentSpanId is null
                    ? null : ActivitySpanId.CreateFromString(entity.MetaData.ParentSpanId),
            ExtraData = entity.MetaData.ExtraData
        }
    };
}