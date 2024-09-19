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

namespace ModularMonolith.Infrastructure.Messaging.SqlServer;

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

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class, IEvent
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

        await _publishEndpoint.Publish(@event, sendContext =>
        {
            sendContext.MessageId = @event.EventId;
            sendContext.Headers.Set("timestamp", @event.Timestamp);
            sendContext.Headers.Set("subject", operationContext.Subject);
            sendContext.Headers.Set("trace_id", operationContext.TraceId.ToString());
            sendContext.Headers.Set("span_id", operationContext.SpanId.ToString());
            sendContext.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }, cancellationToken);

        _logger.EventPublished(eventType.FullName!, @event.EventId);
    }

    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken)
    {
        foreach (var @event in events)
        {
            var eventType = @event.GetType();
            var method = GetType().GetMethods()
                .First(m => m is
                {
                    Name: nameof(this.PublishAsync),
                    IsGenericMethod: true
                });

            var genericMethod = method.MakeGenericMethod(eventType);
            await (Task)genericMethod.Invoke(this, [@event, cancellationToken])!;
        }
    }

    public async Task SendAsync<TCommand>(ICommand command, CancellationToken cancellationToken)
        where TCommand : class, ICommand
    {
        var operationContext = _operationContextAccessor.OperationContext;
        Debug.Assert(operationContext is not null);

        var type = typeof(TCommand);
        var uri = new Uri($"topic: {type.Name}");
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(uri);

        await sendEndpoint.Send<TCommand>(command, sendContext =>
        {
            sendContext.Headers.Set("timestamp", _timeProvider.GetUtcNow());
            sendContext.Headers.Set("subject", operationContext.Subject);
            sendContext.Headers.Set("trace_id", operationContext.TraceId.ToString());
            sendContext.Headers.Set("span_id", operationContext.SpanId.ToString());
            sendContext.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }, cancellationToken);
    }
}
