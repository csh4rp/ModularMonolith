using System.Diagnostics;
using System.Reflection;
using MassTransit;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.DataAccess.EventLog;
using ModularMonolith.Shared.Events;
using ModularMonolith.Shared.Tracing;
using EventLogEntry = ModularMonolith.Shared.DataAccess.EventLog.EventLogEntry;

namespace ModularMonolith.Shared.Messaging.EntityFramework;

internal sealed class MessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpoint _sendEndpoint;
    private readonly IOperationContextAccessor _operationContextAccessor;
    private readonly IEventLogStore _eventLogStore;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MessageBus> _logger;

    public MessageBus(IPublishEndpoint publishEndpoint,
        ISendEndpoint sendEndpoint,
        IOperationContextAccessor operationContextAccessor,
        IEventLogStore eventLogStore,
        TimeProvider timeProvider,
        ILogger<MessageBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _sendEndpoint = sendEndpoint;
        _operationContextAccessor = operationContextAccessor;
        _eventLogStore = eventLogStore;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken)
    {
        var operationContext = _operationContextAccessor.OperationContext;

        Debug.Assert(operationContext is not null);

        var eventType = @event.GetType();
        var eventAttribute = eventType.GetCustomAttribute<EventAttribute>();

        if (eventAttribute?.IsPersisted is true)
        {
            var entry = CreateEventLogEntry(@event, eventType, operationContext);
            await _eventLogStore.AddAsync(entry, cancellationToken);

            _logger.EventPersisted(eventType.FullName!, @event.Id);
        }
        else
        {
            _logger.EventPersistenceSkipped(eventType.FullName!, @event.Id);
        }

        await _publishEndpoint.Publish(@event, eventType, p =>
        {
            p.MessageId = @event.Id;
            p.Headers.Set("timestamp", @event.Timestamp);
            p.Headers.Set("subject", operationContext.Subject);
            p.Headers.Set("trace_id", operationContext.TraceId.ToString());
            p.Headers.Set("span_id", operationContext.SpanId.ToString());
            p.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }, cancellationToken);

        _logger.EventPublished(eventType.FullName!, @event.Id);
    }

    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        var eventsList = events as IReadOnlyCollection<IEvent> ?? events.ToList();

        var operationContext = _operationContextAccessor.OperationContext;

        Debug.Assert(operationContext is not null);

        var eventsToPersist = new List<EventLogEntry>();

        foreach (var @event in eventsList)
        {
            var eventType = @event.GetType();
            var eventAttribute = eventType.GetCustomAttribute<EventAttribute>();

            if (eventAttribute?.IsPersisted is not true)
            {
                _logger.EventPersistenceSkipped(eventType.FullName!, @event.Id);
                continue;
            }

            var entry = CreateEventLogEntry(@event, eventType, operationContext);
            eventsToPersist.Add(entry);
        }

        await _eventLogStore.AddRangeAsync(eventsToPersist, cancellationToken);

        foreach (var @event in eventsList)
        {
            var eventType = @event.GetType();

            await _publishEndpoint.Publish(@event, eventType, p =>
            {
                p.MessageId = @event.Id;
                p.Headers.Set("timestamp", @event.Timestamp);
                p.Headers.Set("subject", operationContext.Subject);
                p.Headers.Set("trace_id", operationContext.TraceId.ToString());
                p.Headers.Set("span_id", operationContext.SpanId.ToString());
                p.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
            }, cancellationToken);

            _logger.EventPublished(eventType.FullName!, @event.Id);
        }
    }

    private static EventLogEntry CreateEventLogEntry(IEvent @event, Type type, IOperationContext operationContext) =>
        new()
        {
            Id = @event.Id,
            Timestamp = @event.Timestamp,
            EventInstance = @event,
            EventType = type,
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

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken)
    {
        var operationContext = _operationContextAccessor.OperationContext;

        Debug.Assert(operationContext is not null);

        await _sendEndpoint.Send(command, command.GetType(), p =>
        {
            p.Headers.Set("timestamp", _timeProvider.GetUtcNow());
            p.Headers.Set("subject", operationContext.Subject);
            p.Headers.Set("trace_id", operationContext.TraceId.ToString());
            p.Headers.Set("span_id", operationContext.SpanId.ToString());
            p.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }, cancellationToken);
    }

}
