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

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

[Collection("Events")]
public class EventPollingBackgroundServiceTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly ActivitySource CurrentActivitySource = new(nameof(EventPollingBackgroundServiceTests));
    private static readonly ActivityListener ActivityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => { },
        ActivityStopped = _ => { },
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };

    private readonly PostgresFixture _postgresFixture;
    private readonly IOptionsMonitor<EventOptions> _eventOptionsMonitor;
    private readonly ILogger<EventPollingBackgroundService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly TimeProvider _timeProvider;
    private readonly DbConnectionFactory _dbConnectionFactory;

    public EventPollingBackgroundServiceTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _timeProvider = Substitute.For<TimeProvider>();
        _eventOptionsMonitor = Substitute.For<IOptionsMonitor<EventOptions>>();
        _logger = Substitute.For<ILogger<EventPollingBackgroundService>>();
        _httpContextAccessor = new HttpContextAccessor();
        _identityContextAccessor = new IdentityContextAccessor
        {
            Context = new IdentityContext(Guid.Parse("5FA82375-60E6-4C57-9E00-C36EA4F954E9"), "mail@mail.com")
        };
        
        _eventOptionsMonitor.CurrentValue.Returns(new EventOptions
        {
            Assemblies = [GetType().Assembly],
            PollInterval = TimeSpan.FromSeconds(1),
            MaxRetryAttempts = 10,
            MaxPollBatchSize = 10,
            MaxEventChannelSize = 10,
            MaxLockTime = TimeSpan.FromSeconds(1)
        });

        var databaseOptionsMonitor = Substitute.For<IOptionsMonitor<DatabaseOptions>>();
        databaseOptionsMonitor.CurrentValue.Returns(new DatabaseOptions
        {
            ConnectionString = _postgresFixture.ConnectionString
        });
        
        _dbConnectionFactory = new DbConnectionFactory(databaseOptionsMonitor);
        _timeProvider.GetUtcNow().Returns(Now);

        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task ShouldSendEventToChannel_WhenSingleEventIsPublished()
    {
        // Arrange
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel(_eventOptionsMonitor);
        var store = CreateEventStore();
        var eventBus = CreateEventBus();
        var service = new EventPollingBackgroundService(_eventOptionsMonitor, _logger, store, channel);
        
        // Act
        await eventBus.PublishAsync(new DomainEvent("Event"), default);

        await service.StartAsync(default);

        // Assert
        await using var enumerator = channel.Reader.ReadAllAsync().GetAsyncEnumerator();
        await enumerator.MoveNextAsync();

        var eventInfo = enumerator.Current;

        var eventLog = await _postgresFixture.SharedDbContext.EventLogs
            .FirstOrDefaultAsync(e => e.Id == eventInfo.EventLogId);

        eventLog.Should().NotBeNull();

        await service.StopAsync(default);
    }

    [Fact]
    public async Task ShouldSendEventsToChannel_WhenMultipleEventsArePublished()
    {
        // Arrange
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel(_eventOptionsMonitor);
        var store = CreateEventStore();
        var eventBus = CreateEventBus();
        var service = new EventPollingBackgroundService(_eventOptionsMonitor, _logger, store, channel);
        var batch = new[] { new DomainEvent("1"), new DomainEvent("2"), new DomainEvent("3"), new DomainEvent("4") };

        // Act
        await eventBus.PublishAsync(batch, default);

        await service.StartAsync(default);

        // Assert
        await using var enumerator = channel.Reader.ReadAllAsync().GetAsyncEnumerator();
        var eventLogIds = new List<Guid>();

        for (var i = 0; i < batch.Length; i++)
        {
            await enumerator.MoveNextAsync();
            eventLogIds.Add(enumerator.Current.EventLogId);
        }

        var eventLogs = await _postgresFixture.SharedDbContext.EventLogs
            .Where(e => eventLogIds.Contains(e.Id))
            .ToListAsync();

        eventLogs.Should().HaveCount(eventLogIds.Count);

        await service.StopAsync(default);
    }

    private OutboxEventBus CreateEventBus() =>
        new(_postgresFixture.SharedDbContext,
            new EventSerializer(_eventOptionsMonitor),
            _identityContextAccessor,
            _timeProvider,
            _httpContextAccessor);

    private EventStore CreateEventStore()
    {
        var provider = new ServiceCollection()
            .AddSingleton<IEventLogDbContext>(_ => _postgresFixture.SharedDbContext)
            .BuildServiceProvider();

        return new EventStore(_dbConnectionFactory,
            new EventMetaDataProvider(provider), 
            _eventOptionsMonitor,
            _timeProvider);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _postgresFixture.ResetAsync();
}