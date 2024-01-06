using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Application.Identity;
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

    private readonly ILogger<EventPublisher> _logger;
    private readonly IEventHandler<DomainEvent> _eventHandler;
    private readonly IEventMapping<DomainEvent> _eventMapping;
    private readonly IEventHandler<IntegrationEvent> _integrationEventHandler;
    private readonly IIdentityContextSetter _identityContextSetter;
    private readonly EventSerializer _eventSerializer;

    public EventPublisherTests()
    {
        _logger = Substitute.For<ILogger<EventPublisher>>();
        _eventHandler = Substitute.For<IEventHandler<DomainEvent>>();
        _eventMapping = Substitute.For<IEventMapping<DomainEvent>>();
        _integrationEventHandler = Substitute.For<IEventHandler<IntegrationEvent>>();
        _identityContextSetter = Substitute.For<IIdentityContextSetter>();

        var eventOptionsMonitor = Substitute.For<IOptionsMonitor<EventOptions>>();
        eventOptionsMonitor.CurrentValue.Returns(new EventOptions { Assemblies = [GetType().Assembly] });

        _eventSerializer = new EventSerializer(eventOptionsMonitor);

        _eventMapping.Map(Arg.Any<DomainEvent>())
            .Returns(c => new IntegrationEvent(c.Arg<DomainEvent>().Name));

        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task ShouldPublishEventToHandler_WhenHandlerIsRegistered()
    {
        // Arrange
        var @event = new DomainEvent("1");

        var provider = new ServiceCollection()
            .AddMediatR(e =>
            {
                e.RegisterServicesFromAssemblies(GetType().Assembly);
            })
            .AddSingleton<INotificationHandler<DomainEvent>>(_ => _eventHandler)
            .AddSingleton<IIdentityContextSetter>(_ => _identityContextSetter)
            .BuildServiceProvider();

        var publisher = new EventPublisher(_eventSerializer, provider, _logger);

        var eventLog = CreateEventLog(@event);

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
        var @event = new DomainEvent("1");

        var provider = new ServiceCollection()
            .AddMediatR(e =>
            {
                e.RegisterServicesFromAssemblies(GetType().Assembly);
            })
            .AddSingleton<IIdentityContextSetter>(_ => _identityContextSetter)
            .BuildServiceProvider();

        var publisher = new EventPublisher(_eventSerializer, provider, _logger);

        var eventLog = CreateEventLog(@event);

        // Act
        await publisher.PublishAsync(eventLog, default);

        // Assert
        await _eventHandler.DidNotReceiveWithAnyArgs().Handle(default!, default);
    }

    [Fact]
    public async Task ShouldPublishMappedEventToHandler_WhenHandlerIsRegisteredAndMappingExists()
    {
        // Arrange
        var @event = new DomainEvent("1");

        var provider = new ServiceCollection()
            .AddMediatR(e =>
            {
                e.RegisterServicesFromAssemblies(GetType().Assembly);
            })
            .AddSingleton<INotificationHandler<DomainEvent>>(_ => _eventHandler)
            .AddSingleton<INotificationHandler<IntegrationEvent>>(_ => _integrationEventHandler)
            .AddSingleton<IEventMapping<DomainEvent>>(_ => _eventMapping)
            .AddSingleton<IIdentityContextSetter>(_ => _identityContextSetter)
            .BuildServiceProvider();

        var publisher = new EventPublisher(_eventSerializer, provider, _logger);

        var eventLog = CreateEventLog(@event);

        // Act
        await publisher.PublishAsync(eventLog, default);

        // Assert
        await _eventHandler.Received(1)
            .Handle(Arg.Is<DomainEvent>(d => d.Equals(@event)), Arg.Any<CancellationToken>());

        await _integrationEventHandler.ReceivedWithAnyArgs(1).Handle(default!, default);
    }

    private static EventLog CreateEventLog(DomainEvent @event) =>
        new()
        {
            Id = Guid.NewGuid(),
            EventType = typeof(DomainEvent).FullName!,
            EventName = nameof(DomainEvent),
            EventPayload = JsonSerializer.Serialize(@event),
            OperationName = "Sample operation",
            TraceId = "",
            SpanId = "",
            Topic = null,
            UserId = Guid.Parse("C77E20AB-A51F-4E86-84D1-E4E13A2D1462"),
            UserName = "mail@mail.com",
            CreatedAt = DateTimeOffset.UtcNow,
            CorrelationId = null,
        };
}
