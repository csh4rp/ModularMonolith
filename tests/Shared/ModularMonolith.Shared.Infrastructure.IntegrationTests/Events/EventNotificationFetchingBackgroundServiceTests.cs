using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using ModularMonolith.Shared.Infrastructure.Events;
using ModularMonolith.Shared.Infrastructure.Events.BackgroundServices;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.Events.Utils;
using ModularMonolith.Shared.Infrastructure.Identity;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Events.Events;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;
using NSubstitute;
using Polly.Registry;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

[Collection("Events")]
public class EventNotificationFetchingBackgroundServiceTests : IAsyncLifetime
{
    private static readonly ActivitySource CurrentActivitySource =
        new(nameof(EventNotificationFetchingBackgroundServiceTests));

    private static readonly ActivityListener ActivityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => { },
        ActivityStopped = _ => { },
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };

    private readonly IOptionsMonitor<EventOptions> _eventOptionsMonitor;
    private readonly ILogger<EventNotificationFetchingBackgroundService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly TimeProvider _timeProvider;
    private readonly PostgresFixture _postgresFixture;
    private readonly DbConnectionFactory _dbConnectionFactory;

    public EventNotificationFetchingBackgroundServiceTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _timeProvider = Substitute.For<TimeProvider>();
        _eventOptionsMonitor = Substitute.For<IOptionsMonitor<EventOptions>>();
        _logger = Substitute.For<ILogger<EventNotificationFetchingBackgroundService>>();
        _httpContextAccessor = new HttpContextAccessor();
        _identityContextAccessor = new IdentityContextAccessor
        {
            Context = new IdentityContext(Guid.Parse("5FA82375-60E6-4C57-9E00-C36EA4F954E9"), "mail@mail.com")
        };
        
        _eventOptionsMonitor.CurrentValue.Returns(new EventOptions
        {
            Assemblies = [GetType().Assembly],
            MaxEventChannelSize = 100,
        });

        var databaseOptionsMonitor = Substitute.For<IOptionsMonitor<DatabaseOptions>>();
        databaseOptionsMonitor.CurrentValue.Returns(new DatabaseOptions
        {
            ConnectionString = _postgresFixture.ConnectionString
        });
        
        _dbConnectionFactory = new DbConnectionFactory(databaseOptionsMonitor);

        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task ShouldSendEventToChannel_WhenSingleEventIsPublished()
    {
        // Arrange
        using var activity = CurrentActivitySource.StartActivity();
        var channel = new EventChannel(_eventOptionsMonitor);
        var eventBus = CreateEventBus();
        using var service = CreateService(channel);
        
        await service.StartAsync(default);

        // Act
        await eventBus.PublishAsync(new DomainEvent("Event"), default);

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
        var eventBus = CreateEventBus();
        var batch = new[] { new DomainEvent("1"), new DomainEvent("2"), new DomainEvent("3"), new DomainEvent("4") };
        using var service = CreateService(channel);

        await service.StartAsync(default);

        // Acy
        await eventBus.PublishAsync(batch, default);

        // Assert
        var eventLogIds = new List<Guid>();
        await using var enumerator = channel.Reader.ReadAllAsync().GetAsyncEnumerator();
        
        for (var i = 0; i < batch.Length; i++)
        {
            await enumerator.MoveNextAsync();
            eventLogIds.Add(enumerator.Current.EventLogId);
        }

        var eventLogs = await _postgresFixture.SharedDbContext.EventLogs.Where(e =>
            eventLogIds.Contains(e.Id)).ToListAsync();

        eventLogs.Should().NotBeEmpty();
        eventLogs.Should().HaveCount(batch.Length);

        await service.StopAsync(default);
    }

    private EventNotificationFetchingBackgroundService CreateService(EventChannel channel) => new(GetPipelineProvider(), _dbConnectionFactory, channel, _logger);

    private OutboxEventBus CreateEventBus() =>
        new(_postgresFixture.SharedDbContext,
            new EventSerializer(_eventOptionsMonitor),
            _identityContextAccessor,
            _timeProvider,
            _httpContextAccessor);
    
    private static ResiliencePipelineProvider<string> GetPipelineProvider()
    {
        var registry = new ResiliencePipelineRegistry<string>();
        registry.TryAddBuilder(EventConstants.EventNotificationFetchingPipelineName, (_, _) => { });

        return registry;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _postgresFixture.ResetAsync();
}
