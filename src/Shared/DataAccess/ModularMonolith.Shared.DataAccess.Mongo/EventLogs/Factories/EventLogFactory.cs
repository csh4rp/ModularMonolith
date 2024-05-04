using System.Diagnostics;
using System.Net;
using ModularMonolith.Shared.DataAccess.EventLogs;
using ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using EventLogEntry = ModularMonolith.Shared.DataAccess.EventLogs.EventLogEntry;

namespace ModularMonolith.Shared.DataAccess.Mongo.EventLogs.Factories;

internal sealed class EventLogFactory
{
    public EventLogEntry Create(EventLogEntity entity)
    {
        var type = Type.GetType(entity.EventTypeName)!;

        return new EventLogEntry
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp,
            Instance = BsonSerializer.Deserialize(entity.EventPayload, type),
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
            EventTypeName = entry.Instance.GetType().FullName!,
            EventPayload = BsonDocument.Create(entry.Instance),
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
