using System.Diagnostics;
using MediatR;
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
public class EventPublisherBackgroundServiceTests
{
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
    
    private readonly ILogger<EventPublisher> _eventPublisherLogger =
        Substitute.For<ILogger<EventPublisher>>();
    
    private readonly INotificationHandler<DomainEvent> _notificationHandler = Substitute.For<INotificationHandler<DomainEvent>>();
    
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
            MaxRetryAttempts = 10
        });
        
        _dbConnectionFactory = new DbConnectionFactory(_databaseOptionsMonitor);
        
        ActivitySource.AddActivityListener(ActivityListener);
    }

    [Fact]
    public async Task Should()
    {
        using var cts = new CancellationTokenSource();
        var channel = new EventChannel();
        var reader = CreateEventReader();

        var scopeFactory = new ServiceCollection()
            .AddMediatR(c =>
            {
                c.Lifetime = ServiceLifetime.Singleton;
                c.RegisterServicesFromAssembly(GetType().Assembly);
            })
            .BuildServiceProvider();
        
        var publisher = new EventPublisher(new EventSerializer(_eventOptionsMonitor), scopeFactory,
            new EventMapper(_eventOptionsMonitor), _eventPublisherLogger);

        var service = new EventPublisherBackgroundService(reader, channel, publisher, _logger, _eventOptionsMonitor);

        var eventBus = CreateEventBus();
        
        await eventBus.PublishAsync(new DomainEvent("1"), cts.Token);

        var eventLog = await _postgresFixture.SharedDbContext.EventLogs.SingleAsync(cts.Token);
        
        var task = service.StartAsync(cts.Token);
        
        // Act
        await channel.WriteAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), cts.Token);
        
        // Assert

        await cts.CancelAsync();
        await task;
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

    private EventReader CreateEventReader()
    {
        var provider = new ServiceCollection()
            .AddSingleton<IEventLogDbContext>(_ => _postgresFixture.SharedDbContext)
            .BuildServiceProvider();

        var reader = new EventReader(_dbConnectionFactory, new EventMetaDataProvider(provider), _eventOptionsMonitor);
        return reader;
    }
}
