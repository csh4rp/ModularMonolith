using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using ModularMonolith.Shared.Infrastructure.Identity;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Events.Events;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;
using NSubstitute;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

[Collection("Events")]
public class EventNotificationFetchingBackgroundServiceTests : IAsyncLifetime
{
    private static readonly ActivitySource CurrentActivitySource = new(nameof(EventNotificationFetchingBackgroundServiceTests));
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

    private readonly ILogger<EventNotificationFetchingBackgroundService> _logger =
        Substitute.For<ILogger<EventNotificationFetchingBackgroundService>>();

    private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    private readonly IIdentityContextAccessor _identityContextAccessor = new IdentityContextAccessor
    {
        Context = new IdentityContext(Guid.Parse("5FA82375-60E6-4C57-9E00-C36EA4F954E9"), "mail@mail.com")
    };

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly PostgresFixture _postgresFixture;
    private readonly DbConnectionFactory _dbConnectionFactory;

    public EventNotificationFetchingBackgroundServiceTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        
        _databaseOptionsMonitor.CurrentValue.Returns(new DatabaseOptions
        {
            ConnectionString = _postgresFixture.ConnectionString
        });

        _eventOptionsMonitor.CurrentValue.Returns(new EventOptions { Assemblies = [GetType().Assembly] });
        
        _dbConnectionFactory = new DbConnectionFactory(_databaseOptionsMonitor);
        
        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task ShouldSendEventToChannel_WhenSingleEventIsPublished()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        using var activitySource = new ActivitySource("Source");
        using var activity = activitySource.StartActivity();
        var channel = new EventChannel();

        var service = new EventNotificationFetchingBackgroundService(_dbConnectionFactory, channel, _logger);

        var eventBus = CreateEventBus();

        var serviceTask = service.StartAsync(cts.Token);
        
        // Act
        await eventBus.PublishAsync(new DomainEvent("Event"), default);

        // Assert
        await using var enumerator = channel.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        await enumerator.MoveNextAsync();

        var eventInfo = enumerator.Current;

        var eventLog = await _postgresFixture.SharedDbContext.EventLogs
            .FirstOrDefaultAsync(e => e.Id == eventInfo.EventLogId, cts.Token);

        eventLog.Should().NotBeNull();

        await cts.CancelAsync();
        await serviceTask;
    }

    private OutboxEventBus CreateEventBus()
    {
        var eventBus = new OutboxEventBus(_postgresFixture.SharedDbContext,
            new EventSerializer(_eventOptionsMonitor),
            _identityContextAccessor,
            _timeProvider,
            _httpContextAccessor);
        return eventBus;
    }

    [Fact]
    public async Task ShouldSendEventsToChannel_WhenMultipleEventsArePublished()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel();

        using var service = new EventNotificationFetchingBackgroundService(_dbConnectionFactory, channel, _logger);

        var eventBus = CreateEventBus();
        
        var batch = new[] { new DomainEvent("1"), new DomainEvent("2"), new DomainEvent("3"), new DomainEvent("4") };

        var serviceTask = service.StartAsync(cts.Token);
        
        // Acy
        await eventBus.PublishAsync(batch, default);

        // Assert
        await using var enumerator = channel.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);

        var eventLogIds = new List<Guid>();

        for (var i = 0; i < batch.Length; i++)
        {
            await enumerator.MoveNextAsync();
            eventLogIds.Add(enumerator.Current.EventLogId);
        }
        
        var eventLogs = await _postgresFixture.SharedDbContext.EventLogs.Where(e =>
            eventLogIds.Contains(e.Id)).ToListAsync(cts.Token);

        eventLogs.Should().NotBeEmpty();
        eventLogs.Should().HaveCount(batch.Length);

        await cts.CancelAsync();
        await serviceTask;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _postgresFixture.ResetAsync();
    }
}
