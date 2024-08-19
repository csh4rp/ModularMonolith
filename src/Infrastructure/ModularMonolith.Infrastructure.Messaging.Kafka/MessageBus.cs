using System.Diagnostics;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Attributes;
using ModularMonolith.Shared.DataAccess.EventLogs;
using ModularMonolith.Shared.Events;
using ModularMonolith.Shared.Messaging;
using ModularMonolith.Shared.Messaging.MassTransit;
using ModularMonolith.Shared.Messaging.MassTransit.Factories;
using ModularMonolith.Shared.Tracing;
using EventLogEntry = ModularMonolith.Shared.DataAccess.EventLogs.EventLogEntry;

namespace ModularMonolith.Infrastructure.Messaging.Kafka;

internal sealed class MessageBus : IMessageBus
{
    private readonly ITopicProducerProvider _topicProducerProvider;
    private readonly IOperationContextAccessor _operationContextAccessor;
    private readonly IEventLogStore _eventLogStore;
    private readonly EventLogEntryFactory _eventLogEntryFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<MessageBus> _logger;

    public MessageBus(ITopicProducerProvider topicProducerProvider, IOperationContextAccessor operationContextAccessor, IEventLogStore eventLogStore, EventLogEntryFactory eventLogEntryFactory, TimeProvider timeProvider, ILogger<MessageBus> logger)
    {
        _topicProducerProvider = topicProducerProvider;
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

        var publisher = GetEventPublisher(@event);
        await publisher.Invoke(@event, cancellationToken);

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
            var publisher = GetEventPublisher(@event);
            await publisher.Invoke(@event, cancellationToken);

            _logger.EventPublished(eventType.FullName!, @event.EventId);
        }
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken)
    {
        var operationContext = _operationContextAccessor.OperationContext;
        Debug.Assert(operationContext is not null);

        var publisher = GetCommandSender(command);
        await publisher.Invoke(command, cancellationToken);
    }

    private Func<IEvent, CancellationToken, Task> GetEventPublisher(IEvent @event)
    {
        var eventType = @event.GetType();
        var topic = eventType.Name;

        var createProducerMethod = _topicProducerProvider.GetType()
            .GetMethod(nameof(ITopicProducerProvider.GetProducer))!
            .MakeGenericMethod(typeof(string), eventType);

        var producer = createProducerMethod.Invoke(_topicProducerProvider, [new Uri($"topic:{topic}")])!;
        var producerType = producer.GetType();

        var method = producerType.GetMethods()
            .Where(m =>
            {
                if (m.Name != nameof(ITopicProducer<string, object>.Produce))
                {
                    return false;
                }

                var parameters = m.GetParameters();

                if (parameters.Length != 3)
                {
                    return false;
                }

                if (parameters[1].ParameterType != eventType)
                {
                    return false;
                }

                return true;
            })
            .First();

        return ((ev, cts) => (Task)method.Invoke(producer, [ev.EventId.ToString(), ev, cts])!);
    }

    private Func<object, CancellationToken, Task> GetCommandSender(ICommand command)
    {
        var commandType = command.GetType();
        var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();
        var topic = commandAttribute?.Target ?? commandType.Name;

        var createProducerMethod = _topicProducerProvider.GetType()
            .GetMethod(nameof(ITopicProducerProvider.GetProducer))!
            .MakeGenericMethod(typeof(string), commandType);

        var producer = createProducerMethod.Invoke(_topicProducerProvider, [new Uri($"topic: {topic}")])!;
        var producerType = producer.GetType();

        var method = producerType.GetMethod(nameof(ITopicProducer<string, object>.Produce))!;

        return ((ev, cts) => (Task)method.Invoke(producer, [Guid.NewGuid().ToString(), ev, cts])!);
    }
}
