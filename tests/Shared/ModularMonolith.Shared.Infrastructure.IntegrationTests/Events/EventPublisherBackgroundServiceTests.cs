﻿using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using ModularMonolith.Shared.Infrastructure.Events;
using ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using ModularMonolith.Shared.Infrastructure.Identity;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Events.Events;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly.Registry;
using EventLog = ModularMonolith.Shared.Domain.Entities.EventLog;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

[Collection("Events")]
public class EventPublisherBackgroundServiceTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly ActivitySource CurrentActivitySource = new(nameof(EventPublisherBackgroundServiceTests));

    private static readonly ActivityListener ActivityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => { },
        ActivityStopped = _ => { },
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };

    private readonly IOptionsMonitor<DatabaseOptions> _databaseOptionsMonitor =
        Substitute.For<IOptionsMonitor<DatabaseOptions>>();

    private readonly IOptionsMonitor<EventOptions> _eventOptionsMonitor =
        Substitute.For<IOptionsMonitor<EventOptions>>();

    private readonly ILogger<EventPublisherBackgroundService> _logger =
        Substitute.For<ILogger<EventPublisherBackgroundService>>();

    private readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();
    private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    private readonly IIdentityContextAccessor _identityContextAccessor = new IdentityContextAccessor
    {
        Context = new IdentityContext(Guid.Parse("5FA82375-60E6-4C57-9E00-C36EA4F954E9"), "mail@mail.com")
    };

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly PostgresFixture _postgresFixture;
    private readonly DbConnectionFactory _dbConnectionFactory;

    public EventPublisherBackgroundServiceTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;

        _databaseOptionsMonitor.CurrentValue.Returns(new DatabaseOptions
        {
            ConnectionString = _postgresFixture.ConnectionString
        });

        _eventOptionsMonitor.CurrentValue.Returns(new EventOptions
        {
            Assemblies = [GetType().Assembly],
            PollInterval = TimeSpan.FromSeconds(1),
            MaxRetryAttempts = 10,
            MaxParallelWorkers = 1,
            MaxPollBatchSize = 10,
            TimeBetweenAttempts = TimeSpan.FromSeconds(5)
        });

        _dbConnectionFactory = new DbConnectionFactory(_databaseOptionsMonitor);

        _timeProvider.GetUtcNow().Returns(Now);

        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task ShouldMarkEventAsPublished_WhenEventIsPublishedSuccessfully()
    {
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel();
        var store = CreateEventStore();

        var service = new EventPublisherBackgroundService(store, channel, _eventPublisher, _logger,
            _eventOptionsMonitor, GetPipelineProvider());
        var eventBus = CreateEventBus();

        await eventBus.PublishAsync(new DomainEvent("1"), default);

        var eventLog = await _postgresFixture.SharedDbContext.EventLogs.SingleAsync();

        await service.StartAsync(default);

        // Act
        await channel.Writer.WriteAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId));

        // Complete the writer, so the background worker will finish 
        channel.Writer.Complete();
        await service.ExecuteTask!;

        // Assert
        eventLog = await _postgresFixture.SharedDbContext.EventLogs
            .AsNoTracking()
            .SingleAsync(e => e.Id == eventLog.Id);

        eventLog.PublishedAt.Should().NotBeNull();

        await _eventPublisher.Received(1)
            .PublishAsync(Arg.Is<EventLog>(e => e.Id == eventLog.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldMarkEventsAsPublished_WhenEventsArePublishedSuccessfully()
    {
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel();
        var store = CreateEventStore();

        var service = new EventPublisherBackgroundService(store, channel, _eventPublisher, _logger,
            _eventOptionsMonitor, GetPipelineProvider());
        var eventBus = CreateEventBus();

        var batch = new[] { new DomainEvent("1"), new DomainEvent("2"), new DomainEvent("3"), new DomainEvent("4") };

        await eventBus.PublishAsync(batch, default);

        var eventLogs = await _postgresFixture.SharedDbContext.EventLogs.ToListAsync();

        await service.StartAsync(default);

        // Act
        foreach (var eventLog in eventLogs)
        {
            await channel.Writer.WriteAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId));
        }

        // Complete the writer, so the background worker will finish 
        channel.Writer.Complete();
        await service.ExecuteTask!;

        // Assert
        var ids = eventLogs.Select(e => e.Id).ToList();

        eventLogs = await _postgresFixture.SharedDbContext.EventLogs
            .AsNoTracking()
            .ToListAsync();

        eventLogs.Should().HaveCount(batch.Length);
        eventLogs.Should().AllSatisfy(e => e.PublishedAt.Should().NotBeNull());

        await _eventPublisher.Received(batch.Length)
            .PublishAsync(Arg.Is<EventLog>(e => ids.Contains(e.Id)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldAddFailedAttempt_WhenEventPublicationFails()
    {
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel();
        var store = CreateEventStore();
        _eventPublisher.PublishAsync(default!, default).ThrowsAsyncForAnyArgs<InvalidOperationException>();

        var service = new EventPublisherBackgroundService(store, channel, _eventPublisher, _logger,
            _eventOptionsMonitor, GetPipelineProvider());
        var eventBus = CreateEventBus();

        await eventBus.PublishAsync(new DomainEvent("1"), default);

        var eventLog = await _postgresFixture.SharedDbContext.EventLogs.SingleAsync();

        await service.StartAsync(default);

        // Act
        await channel.Writer.WriteAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId));

        // Complete the writer, so the background worker will finish 
        channel.Writer.Complete();
        await service.ExecuteTask!;

        // Assert
        eventLog = await _postgresFixture.SharedDbContext.EventLogs
            .AsNoTracking()
            .SingleAsync(e => e.Id == eventLog.Id);

        var attempt = await _postgresFixture.SharedDbContext.EventLogPublishAttempts
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.EventLogId == eventLog.Id);

        eventLog.PublishedAt.Should().BeNull();

        attempt.Should().NotBeNull();
        attempt!.AttemptNumber.Should().Be(1);

        await _eventPublisher.Received(1)
            .PublishAsync(Arg.Is<EventLog>(e => e.Id == eventLog.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldAddFailedAttempts_WhenEventsPublicationFails()
    {
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel();
        var store = CreateEventStore();
        _eventPublisher.PublishAsync(default!, default).ThrowsAsyncForAnyArgs<InvalidOperationException>();

        var service = new EventPublisherBackgroundService(store, channel, _eventPublisher, _logger,
            _eventOptionsMonitor, GetPipelineProvider());
        var eventBus = CreateEventBus();

        var batch = new[] { new DomainEvent("1"), new DomainEvent("2"), new DomainEvent("3"), new DomainEvent("4") };

        await eventBus.PublishAsync(batch, default);

        var eventLogs = await _postgresFixture.SharedDbContext.EventLogs.ToListAsync();

        await service.StartAsync(default);

        // Act
        foreach (var eventLog in eventLogs)
        {
            await channel.Writer.WriteAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId));
        }

        // Complete the writer, so the background worker will finish 
        channel.Writer.Complete();
        await service.ExecuteTask!;

        // Assert
        eventLogs = await _postgresFixture.SharedDbContext.EventLogs
            .AsNoTracking()
            .ToListAsync();

        var ids = eventLogs.Select(e => e.Id).ToList();

        var attempts = await _postgresFixture.SharedDbContext.EventLogPublishAttempts
            .AsNoTracking()
            .Where(e => ids.Contains(e.EventLogId))
            .ToListAsync();

        eventLogs.Should().HaveCount(batch.Length);
        eventLogs.Should().AllSatisfy(e => e.PublishedAt.Should().BeNull());

        attempts.Should().HaveCount(batch.Length);
        attempts.Should().AllSatisfy(e => e.AttemptNumber.Should().Be(1));

        await _eventPublisher.Received(batch.Length)
            .PublishAsync(Arg.Is<EventLog>(e => ids.Contains(e.Id)), Arg.Any<CancellationToken>());
    }

    private EventStore CreateEventStore()
    {
        var provider = new ServiceCollection()
            .AddSingleton<IEventLogDbContext>(_ => _postgresFixture.SharedDbContext)
            .BuildServiceProvider();

        return new EventStore(_dbConnectionFactory, new EventMetaDataProvider(provider), _eventOptionsMonitor,
            _timeProvider);
    }

    private OutboxEventBus CreateEventBus() =>
        new(_postgresFixture.SharedDbContext,
            new EventSerializer(_eventOptionsMonitor),
            _identityContextAccessor,
            _timeProvider,
            _httpContextAccessor);

    private static ResiliencePipelineProvider<string> GetPipelineProvider()
    {
        var registry = new ResiliencePipelineRegistry<string>();

        registry.TryAddBuilder(EventConstants.ReceiverPipelineName, (_, _) => { });
        registry.TryAddBuilder(EventConstants.EventLockReleasePipelineName, (_, _) => { });
        registry.TryAddBuilder(EventConstants.EventPublicationPipelineName, (_, _) => { });

        return registry;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        return _postgresFixture.ResetAsync();
    }
}
