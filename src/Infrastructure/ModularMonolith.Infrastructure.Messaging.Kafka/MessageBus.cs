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

namespace ModularMonolith.Infrastructure.Messaging.Kafka;

internal sealed class MessageBus : IMessageBus
{
    private readonly ITopicProducerProvider _topicProducerProvider;
    private readonly IOperationContextAccessor _operationContextAccessor;
    private readonly IEventLogStore _eventLogStore;
    private readonly EventLogEntryFactory _eventLogEntryFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MessageBus> _logger;

    public MessageBus(ITopicProducerProvider topicProducerProvider,
        IOperationContextAccessor operationContextAccessor,
        IEventLogStore eventLogStore,
        EventLogEntryFactory eventLogEntryFactory,
        TimeProvider timeProvider,
        ILogger<MessageBus> logger)
    {
        _topicProducerProvider = topicProducerProvider;
        _operationContextAccessor = operationContextAccessor;
        _eventLogStore = eventLogStore;
        _eventLogEntryFactory = eventLogEntryFactory;
        _timeProvider = timeProvider;
        _logger = logger;
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

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class, IEvent
    {
        var operationContext = _operationContextAccessor.OperationContext;
        Debug.Assert(operationContext is not null);

        var topic =  DefaultEndpointNameFormatter.Instance.Message<TEvent>();
        var type = typeof(TEvent);
        var uri = new Uri($"topic:{topic}");
        var producer = _topicProducerProvider.GetProducer<Guid, TEvent>(uri);
        var eventAttribute = type.GetCustomAttribute<EventAttribute>();

        if (eventAttribute?.IsPersisted is true)
        {
            var entry = _eventLogEntryFactory.Create(@event);
            await _eventLogStore.AddAsync(entry, cancellationToken);

            _logger.EventPersisted(type.FullName!, @event.EventId);
        }
        else
        {
            _logger.EventPersistenceSkipped(type.FullName!, @event.EventId);
        }

        await producer.Produce(@event.EventId, @event, Pipe.Execute<KafkaSendContext<Guid, TEvent>>(kafkaCnx =>
        {
            kafkaCnx.MessageId = @event.EventId;
            kafkaCnx.Headers.Set("timestamp", @event.Timestamp);
            kafkaCnx.Headers.Set("subject", operationContext.Subject);
            kafkaCnx.Headers.Set("trace_id", operationContext.TraceId.ToString());
            kafkaCnx.Headers.Set("span_id", operationContext.SpanId.ToString());
            kafkaCnx.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }), cancellationToken);
    }

    public Task SendAsync<TCommand>(ICommand command, CancellationToken cancellationToken)
        where TCommand : class, ICommand
    {
        var operationContext = _operationContextAccessor.OperationContext;
        Debug.Assert(operationContext is not null);

        var topic =  DefaultEndpointNameFormatter.Instance.Message<TCommand>();
        var uri = new Uri($"topic:{topic}");;
        var producer = _topicProducerProvider.GetProducer<Guid, TCommand>(uri);

        return producer.Produce(Guid.NewGuid(), command, Pipe.Execute<KafkaSendContext<Guid, TCommand>>(kafkaCnx =>
        {
            kafkaCnx.Headers.Set("timestamp", _timeProvider.GetUtcNow());
            kafkaCnx.Headers.Set("subject", operationContext.Subject);
            kafkaCnx.Headers.Set("trace_id", operationContext.TraceId.ToString());
            kafkaCnx.Headers.Set("span_id", operationContext.SpanId.ToString());
            kafkaCnx.Headers.Set("parent_span_id", operationContext.ParentSpanId.ToString());
        }), cancellationToken);
    }
}
