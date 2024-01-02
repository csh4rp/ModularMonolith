using System.Diagnostics;
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
public class EventPollingBackgroundServiceTests
{
    private readonly IOptionsMonitor<DatabaseOptions> _databaseOptionsMonitor =
        Substitute.For<IOptionsMonitor<DatabaseOptions>>();

    private readonly IOptionsMonitor<EventOptions> _eventOptionsMonitor =
        Substitute.For<IOptionsMonitor<EventOptions>>();

    private readonly ILogger<EventPollingBackgroundService> _logger =
        Substitute.For<ILogger<EventPollingBackgroundService>>();

    private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    private readonly IIdentityContextAccessor _identityContextAccessor = new IdentityContextAccessor
    {
        Context = new IdentityContext(Guid.Parse("5FA82375-60E6-4C57-9E00-C36EA4F954E9"), "mail@mail.com")
    };

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly PostgresFixture _postgresFixture;
    private readonly DbConnectionFactory _dbConnectionFactory;

    private static readonly ActivityListener ActivityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => { },
        ActivityStopped = _ => { },
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };
    
    public EventPollingBackgroundServiceTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        
        _databaseOptionsMonitor.CurrentValue.Returns(new DatabaseOptions
        {
            ConnectionString = _postgresFixture.ConnectionString
        });

        _eventOptionsMonitor.CurrentValue.Returns(new EventOptions
        {
            Assemblies = [GetType().Assembly],
            PollInterval = TimeSpan.FromSeconds(1)
        });
        
        _dbConnectionFactory = new DbConnectionFactory(_databaseOptionsMonitor);
        
        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task Should()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        using var activitySource = new ActivitySource("Source");
        using var activity = activitySource.StartActivity();
        var channel = new EventChannel();

        var sc = new ServiceCollection();
        var cnx = _postgresFixture.SharedDbContext;
        sc.AddSingleton<IEventLogDbContext>(_ => cnx);

        var reader = new EventReader(_dbConnectionFactory, 
            new EventMetaDataProvider(sc.BuildServiceProvider()),
            _eventOptionsMonitor);

        var service = new EventPollingBackgroundService(_eventOptionsMonitor, _logger, reader, channel);

        var eventBus = new OutboxEventBus(_postgresFixture.SharedDbContext,
            new EventSerializer(_eventOptionsMonitor),
            _identityContextAccessor,
            _timeProvider,
            _httpContextAccessor);

        // Act
        await eventBus.PublishAsync(new DomainEvent("Event"), default);
        
        var serviceTask = service.StartAsync(cts.Token);

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
}
