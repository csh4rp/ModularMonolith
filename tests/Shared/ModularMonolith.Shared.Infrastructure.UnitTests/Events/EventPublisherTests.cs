using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Domain.Abstractions;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using NSubstitute;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.UnitTests.Events;

public class EventPublisherTests
{
    private static readonly ActivityListener ActivityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => { },
        ActivityStopped = _ => { },
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };

    private readonly IOptionsMonitor<EventOptions> _eventOptionsMonitor = Substitute.For<IOptionsMonitor<EventOptions>>();
    private readonly ILogger<EventPublisher> _logger = Substitute.For<ILogger<EventPublisher>>();
    private readonly IEventHandler<DomainEvent> _eventHandler = Substitute.For<IEventHandler<DomainEvent>>();
    private readonly IEventMapping<DomainEvent> _eventMapping = Substitute.For<IEventMapping<DomainEvent>>();
    private readonly IEventHandler<IntegrationEvent> _integrationEventHandler = Substitute.For<IEventHandler<IntegrationEvent>>();

    private readonly EventSerializer _eventSerializer;

    public EventPublisherTests()
    {
        _eventOptionsMonitor.CurrentValue.Returns(new EventOptions { Assemblies = [GetType().Assembly] });

        _eventSerializer = new EventSerializer(_eventOptionsMonitor);

        _eventMapping.Map(Arg.Any<DomainEvent>())
            .Returns(c => new IntegrationEvent(c.Arg<DomainEvent>().Name));

        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task ShouldPublishEventToHandler_WhenHandlerIsRegistered()
    {
        // Arrange
        var provider = new ServiceCollection()
            .AddMediatR(e =>
            {
                e.RegisterServicesFromAssemblies(GetType().Assembly);
            })
            .AddSingleton<INotificationHandler<DomainEvent>>(_ => _eventHandler)
            .BuildServiceProvider();

        var publisher = new EventPublisher(_eventSerializer, provider, _logger);

        var @event = new DomainEvent("1");

        var eventLog = new EventLog
        {
            Id = Guid.NewGuid(),
            EventType = typeof(DomainEvent).FullName!,
            EventName = nameof(DomainEvent),
            EventPayload = JsonSerializer.Serialize(@event),
            OperationName = "Sample operation",
            TraceId = "",
            SpanId = "",
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = null,
        };

        // Act
        await publisher.PublishAsync(eventLog, default);

        // Assert
        await _eventHandler.Received(1)
            .Handle(Arg.Is<DomainEvent>(d => d.Equals(@event)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldNotPublishEventToHandler_WhenHandlerIsNotRegistered()
    {
        // Arrange
        var provider = new ServiceCollection()
            .AddMediatR(e =>
            {
                e.RegisterServicesFromAssemblies(GetType().Assembly);
            })
            .BuildServiceProvider();

        var publisher = new EventPublisher(_eventSerializer, provider, _logger);

        var @event = new DomainEvent("1");

        var eventLog = new EventLog
        {
            Id = Guid.NewGuid(),
            EventType = typeof(DomainEvent).FullName!,
            EventName = nameof(DomainEvent),
            EventPayload = JsonSerializer.Serialize(@event),
            OperationName = "Sample operation",
            TraceId = "",
            SpanId = "",
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = null,
        };

        // Act
        await publisher.PublishAsync(eventLog, default);

        // Assert
        await _eventHandler.DidNotReceiveWithAnyArgs().Handle(default!, default);
    }

    [Fact]
    public async Task ShouldPublishMappedEventToHandler_WhenHandlerIsRegisteredAndMappingExists()
    {
        // Arrange
        var provider = new ServiceCollection()
            .AddMediatR(e =>
            {
                e.RegisterServicesFromAssemblies(GetType().Assembly);
            })
            .AddSingleton<INotificationHandler<DomainEvent>>(_ => _eventHandler)
            .AddSingleton<INotificationHandler<IntegrationEvent>>(_ => _integrationEventHandler)
            .AddSingleton<IEventMapping<DomainEvent>>(_ => _eventMapping)
            .BuildServiceProvider();

        var publisher = new EventPublisher(_eventSerializer, provider, _logger);

        var @event = new DomainEvent("1");

        var eventLog = new EventLog
        {
            Id = Guid.NewGuid(),
            EventType = typeof(DomainEvent).FullName!,
            EventName = nameof(DomainEvent),
            EventPayload = JsonSerializer.Serialize(@event),
            OperationName = "Sample operation",
            TraceId = "",
            SpanId = "",
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = null,
        };

        // Act
        await publisher.PublishAsync(eventLog, default);

        // Assert
        await _eventHandler.Received(1)
            .Handle(Arg.Is<DomainEvent>(d => d.Equals(@event)), Arg.Any<CancellationToken>());

        await _integrationEventHandler.ReceivedWithAnyArgs(1).Handle(default!, default);
    }
}
