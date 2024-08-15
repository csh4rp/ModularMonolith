using System.Diagnostics;
using System.Net;
using System.Text.Json;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLog.Models;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.Models;
using ModularMonolith.Shared.DataAccess.EventLogs;
using EventLogEntry = ModularMonolith.Shared.DataAccess.EventLogs.EventLogEntry;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.Factories;

internal sealed class EventLogFactory
{
    public EventLogEntry Create(EventLogEntity entity)
    {
        var type = Type.GetType(entity.EventTypeName)!;

        return new EventLogEntry
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp,
            Instance = entity.EventPayload.Deserialize(type)!,
            MetaData = new EventLogEntryMetaData
            {
                Subject = entity.MetaData.Subject,
                Uri = entity.MetaData.Uri is null ? null : new Uri(entity.MetaData.Uri),
                IpAddress = entity.MetaData.IpAddress is null ? null : IPAddress.Parse(entity.MetaData.IpAddress),
                OperationName = entity.MetaData.OperationName,
                TraceId =
                    entity.MetaData.TraceId is null ? null : ActivityTraceId.CreateFromString(entity.MetaData.TraceId),
                SpanId =
                    entity.MetaData.SpanId is null ? null : ActivitySpanId.CreateFromString(entity.MetaData.SpanId),
                ParentSpanId = entity.MetaData.ParentSpanId is null
                    ? null
                    : ActivitySpanId.CreateFromString(entity.MetaData.ParentSpanId),
            }
        };
    }

    public EventLogEntity Create(EventLogEntry entry) =>
        new()
        {
            Id = entry.Id,
            Timestamp = entry.Timestamp,
            EventPayload = JsonSerializer.SerializeToDocument(entry.Instance),
            EventTypeName = entry.Instance.GetType().FullName!,
            MetaData = new EventLogEntityMetaData
            {
                Subject = entry.MetaData.Subject,
                Uri = entry.MetaData.Uri?.ToString(),
                IpAddress = entry.MetaData.IpAddress?.ToString(),
                OperationName = entry.MetaData.OperationName,
                TraceId = entry.MetaData.TraceId?.ToString(),
                SpanId = entry.MetaData.SpanId?.ToString(),
                ParentSpanId = entry.MetaData.ParentSpanId?.ToString()
            }
        };
}
