using System.Net;
using System.Text.Json;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLog.Models;
using ModularMonolith.Shared.DataAccess.EventLog;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLog.Factories;

internal sealed class EventLogFactory
{
    public EventLogEntry Create(EventLogEntity entity)
    {
        var type = Type.GetType(entity.EventTypeName)!;

        return new EventLogEntry
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp,
            EventInstance = entity.EventPayload.Deserialize(type)!,
            EventType = type,
            MetaData = new EventLogEntryMetaData
            {
                Subject = entity.MetaData.Subject,
                Uri = entity.MetaData.Uri is null ? null : new Uri(entity.MetaData.Uri),
                IpAddress = entity.MetaData.IpAddress is null ? null : IPAddress.Parse(entity.MetaData.IpAddress),
                OperationName = entity.MetaData.OperationName,
                TraceId = entity.MetaData.TraceId,
                SpanId = entity.MetaData.SpanId,
                ParentSpanId = entity.MetaData.ParentSpanId,
                CorrelationId = entity.MetaData.CorrelationId
            }
        };
    }

    public EventLogEntity Create(EventLogEntry entry) =>
        new()
        {
            Id = entry.Id,
            Timestamp = entry.Timestamp,
            EventPayload = JsonSerializer.SerializeToDocument(entry.EventInstance),
            EventTypeName = entry.EventType.FullName!,
            MetaData = new EventLogEntityMetaData
            {
                Subject = entry.MetaData.Subject,
                Uri = entry.MetaData.Uri?.ToString(),
                IpAddress = entry.MetaData.IpAddress?.ToString(),
                OperationName = entry.MetaData.OperationName,
                TraceId = entry.MetaData.TraceId,
                SpanId = entry.MetaData.SpanId,
                ParentSpanId = entry.MetaData.ParentSpanId,
                CorrelationId = entry.MetaData.CorrelationId
            }
        };
}
