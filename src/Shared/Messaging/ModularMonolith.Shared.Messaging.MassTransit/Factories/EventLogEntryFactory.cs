using System.Diagnostics;
using ModularMonolith.Shared.DataAccess.EventLogs;
using ModularMonolith.Shared.Events;
using ModularMonolith.Shared.Tracing;
using EventLogEntry = ModularMonolith.Shared.DataAccess.EventLogs.EventLogEntry;

namespace ModularMonolith.Shared.Messaging.MassTransit.Factories;

internal sealed class EventLogEntryFactory
{
    private readonly IOperationContextAccessor _operationContextAccessor;

    public EventLogEntryFactory(IOperationContextAccessor operationContextAccessor) =>
        _operationContextAccessor = operationContextAccessor;

    public EventLogEntry Create(IEvent @event)
    {
        var operationContext = _operationContextAccessor.OperationContext;
        Debug.Assert(operationContext is not null);

        return new()
        {
            Id = @event.EventId,
            Timestamp = @event.Timestamp,
            Instance = @event,
            MetaData = new EventLogEntryMetaData
            {
                Subject = operationContext.Subject,
                Uri = operationContext.Uri,
                IpAddress = operationContext.IpAddress,
                OperationName = operationContext.OperationName,
                TraceId = operationContext.TraceId,
                SpanId = operationContext.SpanId,
                ParentSpanId = operationContext.ParentSpanId
            }
        };
    }

}
