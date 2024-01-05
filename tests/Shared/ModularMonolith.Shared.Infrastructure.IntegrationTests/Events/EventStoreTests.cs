using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.DataAccess.Factories;
using ModularMonolith.Shared.Infrastructure.DataAccess.Options;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Concrete;
using ModularMonolith.Shared.Infrastructure.Events.MetaData;
using ModularMonolith.Shared.Infrastructure.Events.Options;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Events.Events;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;
using NSubstitute;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Events;

[Collection("Events")]
public class EventStoreTests
{
    private static readonly DateTimeOffset Now = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private readonly IOptionsMonitor<DatabaseOptions> _databaseOptionsMonitor = Substitute.For<IOptionsMonitor<DatabaseOptions>>();
    private readonly IOptionsMonitor<EventOptions> _eventOptionsMonitor = Substitute.For<IOptionsMonitor<EventOptions>>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly PostgresFixture _postgresFixture;

    public EventStoreTests(PostgresFixture postgresFixture)
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
    }

    [Fact]
    public async Task ShouldReturnEvent_WhenItsUnpublished()
    {
        // Arrange
        var store = CreateEventStore();

        var eventLog = new EventLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = _timeProvider.GetUtcNow(),
            EventName = nameof(DomainEvent),
            EventPayload = JsonSerializer.Serialize(new DomainEvent("1")),
            EventType = typeof(DomainEvent).FullName!,
            TraceId = "1",
            SpanId = "1",
            OperationName = "Event Creation",
            CorrelationId = null
        };

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        var events = await store.GetUnpublishedEventsAsync(int.MaxValue, default);
        
        // Assert
        events.Should().HaveCount(1);
        events[0].EventLogId.Should().Be(eventLog.Id);
    }
    
    [Fact]
    public async Task ShouldAddFirstFailedAttempt_WhenFailedAttemptDoesNotExist()
    {
        // Arrange
        var store = CreateEventStore();

        var eventLog = new EventLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = _timeProvider.GetUtcNow(),
            EventName = nameof(DomainEvent),
            EventPayload = JsonSerializer.Serialize(new DomainEvent("1")),
            EventType = typeof(DomainEvent).FullName!,
            TraceId = "1",
            SpanId = "1",
            OperationName = "Event Creation",
            CorrelationId = null
        };

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        await store.AddFailedAttemptAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);
        
        // Assert
        var attempt = await _postgresFixture.SharedDbContext.EventLogPublishAttempts.SingleOrDefaultAsync();

        attempt.Should().NotBeNull();
        attempt!.AttemptNumber.Should().Be(1);
        attempt.EventLogId.Should().Be(eventLog.Id);
    }
    
    [Fact]
    public async Task ShouldAddSecondFailedAttempt_WhenFailedAttemptExists()
    {
        // Arrange
        var store = CreateEventStore();

        var eventLog = new EventLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = _timeProvider.GetUtcNow(),
            EventName = nameof(DomainEvent),
            EventPayload = JsonSerializer.Serialize(new DomainEvent("1")),
            EventType = typeof(DomainEvent).FullName!,
            TraceId = "1",
            SpanId = "1",
            OperationName = "Event Creation",
            CorrelationId = null
        };

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        await store.AddFailedAttemptAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);
        await store.AddFailedAttemptAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);
        
        // Assert
        var attempt = await _postgresFixture.SharedDbContext.EventLogPublishAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.AttemptNumber == 2);

        attempt.Should().NotBeNull();
        attempt!.AttemptNumber.Should().Be(2);
        attempt.EventLogId.Should().Be(eventLog.Id);
    }
    
    private EventStore CreateEventStore()
    {
        var provider = new ServiceCollection()
            .AddSingleton<IEventLogDbContext>(_ => _postgresFixture.SharedDbContext)
            .BuildServiceProvider();

        return new EventStore(_dbConnectionFactory, new EventMetaDataProvider(provider), _eventOptionsMonitor, _timeProvider);
    }
}
