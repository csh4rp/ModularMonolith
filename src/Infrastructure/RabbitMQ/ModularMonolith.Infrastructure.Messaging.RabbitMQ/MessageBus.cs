using System.Diagnostics;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.DataAccess.EventLogs;
using ModularMonolith.Shared.Events;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Messaging.MassTransit;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;
using ModularMonolith.Shared.Tracing;
using EventLogEntry = ModularMonolith.Shared.DataAccess.EventLogs.EventLogEntry;

namespace ModularMonolith.Infrastructure.Messaging.RabbitMQ;

internal sealed class MessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IOperationContextAccessor _operationContextAccessor;
    private readonly IEventLogStore _eventLogStore;
    private readonly EventLogEntryFactory _eventLogEntryFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MessageBus> _logger;

    public MessageBus(IPublishEndpoint publishEndpoint,
        ISendEndpointProvider sendEndpointProvider,
        IOperationContextAccessor operationContextAccessor,
        IEventLogStore eventLogStore,
        EventLogEntryFactory eventLogEntryFactory,
        TimeProvider timeProvider,
        ILogger<MessageBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _sendEndpointProvider = sendEndpointProvider;
        _operationContextAccessor = operationContextAccessor;
        _eventLogStore = eventLogStore;
        _eventLogEntryFactory = eventLogEntryFactory;
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
            var entry = _eventLogEntryFactory.Create(@event);
            await _eventLogStore.AddAsync(entry, cancellationToken);

            _logger.EventPersisted(eventType.FullName!, @event.EventId);
        }
        else
        {
            _logger.EventPersistenceSkipped(eventType.FullName!, @event.EventId);
        }

        await _publishEndpoint.Publish(@event, eventType, p =>
        {
            p.MessageId = @event.EventId;
            p.Headers.Set("timestamp", @event.Timestamp);
            p.Headers.Set("subject", operationContext.Subject);
            p.Headers.Set("trace_id", operationContext.TraceId.ToString());
            p.Headers.Set("span_id", operationContext.SpanId.ToString());
            p.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }, cancellationToken);

        _logger.EventPublished(eventType.FullName!, @event.EventId);
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
                _logger.EventPersistenceSkipped(eventType.FullName!, @event.EventId);
                continue;
            }

            var entry = _eventLogEntryFactory.Create(@event);
            eventsToPersist.Add(entry);
        }

        await _eventLogStore.AddRangeAsync(eventsToPersist, cancellationToken);

        _logger.EventsPersisted(eventsToPersist.Select(e => e.Id));

        foreach (var @event in eventsList)
        {
            var eventType = @event.GetType();

            await _publishEndpoint.Publish(@event, eventType, p =>
            {
                p.MessageId = @event.EventId;
                p.Headers.Set("timestamp", @event.Timestamp);
                p.Headers.Set("subject", operationContext.Subject);
                p.Headers.Set("trace_id", operationContext.TraceId.ToString());
                p.Headers.Set("span_id", operationContext.SpanId.ToString());
                p.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
            }, cancellationToken);

            _logger.EventPublished(eventType.FullName!, @event.EventId);
        }
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken)
    {
        var operationContext = _operationContextAccessor.OperationContext;
        Debug.Assert(operationContext is not null);

        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(""));

        await sendEndpoint.Send(command, command.GetType(), p =>
        {
            p.Headers.Set("timestamp", _timeProvider.GetUtcNow());
            p.Headers.Set("subject", operationContext.Subject);
            p.Headers.Set("trace_id", operationContext.TraceId.ToString());
            p.Headers.Set("span_id", operationContext.SpanId.ToString());
            p.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }, cancellationToken);
    }
}
