using System.Text.Json;
using Bogus;
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
public class EventStoreTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly PostgresFixture _postgresFixture;
    private readonly IOptionsMonitor<EventOptions> _eventOptionsMonitor;
    private readonly TimeProvider _timeProvider;
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly Faker<EventLog> _eventLogFaker;

    public EventStoreTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _timeProvider = Substitute.For<TimeProvider>();
        _eventOptionsMonitor = Substitute.For<IOptionsMonitor<EventOptions>>();
        _eventLogFaker = new Faker<EventLog>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.EventType, typeof(DomainEvent).FullName!)
            .RuleFor(x => x.EventName, nameof(DomainEvent))
            .RuleFor(x => x.CreatedAt, Now)
            .RuleFor(x => x.TraceId, f => f.Random.String2(32))
            .RuleFor(x => x.SpanId, f => f.Random.String2(16))
            .RuleFor(x => x.OperationName, "Event Creation")
            .RuleFor(x => x.EventPayload, JsonSerializer.Serialize(new DomainEvent("1")));

        _eventOptionsMonitor.CurrentValue.Returns(new EventOptions
        {
            Assemblies = [GetType().Assembly],
            PollInterval = TimeSpan.FromSeconds(1),
            MaxRetryAttempts = 10,
            MaxParallelWorkers = 1,
            MaxPollBatchSize = 10,
            TimeBetweenAttempts = TimeSpan.FromSeconds(5)
        });

        var databaseOptionsMonitor = Substitute.For<IOptionsMonitor<DatabaseOptions>>();
        databaseOptionsMonitor.CurrentValue.Returns(new DatabaseOptions
        {
            ConnectionString = _postgresFixture.ConnectionString
        });

        _dbConnectionFactory = new DbConnectionFactory(databaseOptionsMonitor);
        _timeProvider.GetUtcNow().Returns(Now);
    }

    [Fact]
    public async Task ShouldReturnEvent_WhenItsUnpublished()
    {
        // Arrange
        var store = CreateEventStore();
        var eventLog = _eventLogFaker.Generate();

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
        var eventLog = _eventLogFaker.Generate();

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
        var eventLog = _eventLogFaker.Generate();

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

    [Fact]
    public async Task ShouldMarkAsPublished_WhenEventIsNotPublished()
    {
        // Arrange
        var store = CreateEventStore();
        var eventLog = _eventLogFaker.Generate();

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        await store.MarkAsPublishedAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);

        // Assert
        eventLog = await _postgresFixture.SharedDbContext.EventLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventLog.Id);

        eventLog.Should().NotBeNull();
        eventLog!.PublishedAt.Should().Be(Now);
    }

    [Fact]
    public async Task ShouldAcquireLock_WhenNoLockIsTaken()
    {
        // Arrange
        var store = CreateEventStore();
        var eventLog = _eventLogFaker.Generate();

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        var (acquired, _) =
            await store.TryAcquireLockAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);

        // Assert
        acquired.Should().BeTrue();

        var eventLock = await _postgresFixture.SharedDbContext.EventLogLocks.SingleOrDefaultAsync();
        var correlationLock = await _postgresFixture.SharedDbContext.EventCorrelationLocks.SingleOrDefaultAsync();

        eventLock.Should().NotBeNull();
        eventLock!.EventLogId.Should().Be(eventLog.Id);

        correlationLock.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotAcquireLock_WhenLockIsTaken()
    {
        // Arrange
        var store = CreateEventStore();
        var eventLog = _eventLogFaker.Generate();

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        _postgresFixture.SharedDbContext.EventLogLocks.Add(new EventLogLock
        {
            EventLogId = eventLog.Id, AcquiredAt = Now
        });
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        var (acquired, _) =
            await store.TryAcquireLockAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);

        // Assert
        acquired.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldAcquireCorrelationLock_WhenNoLockIsTaken()
    {
        // Arrange
        var store = CreateEventStore();
        var eventLog = _eventLogFaker.Clone()
            .RuleFor(x => x.CorrelationId, f => f.Random.Guid())
            .Generate();

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        var (acquired, _) =
            await store.TryAcquireLockAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);

        // Assert
        acquired.Should().BeTrue();

        var eventLock = await _postgresFixture.SharedDbContext.EventLogLocks.SingleOrDefaultAsync();
        var correlationLock = await _postgresFixture.SharedDbContext.EventCorrelationLocks.SingleOrDefaultAsync();

        eventLock.Should().BeNull();

        correlationLock.Should().NotBeNull();
        correlationLock!.CorrelationId.Should().Be(eventLog.CorrelationId!.Value);
    }

    [Fact]
    public async Task ShouldNotAcquireCorrelationLock_WhenCorrelationLockIsTaken()
    {
        // Arrange
        var store = CreateEventStore();
        var eventLog = _eventLogFaker.Clone()
            .RuleFor(x => x.CorrelationId, f => f.Random.Guid())
            .Generate();

        _postgresFixture.SharedDbContext.EventLogs.Add(eventLog);
        _postgresFixture.SharedDbContext.EventCorrelationLocks.Add(new EventCorrelationLock
        {
            CorrelationId = eventLog.CorrelationId!.Value, AcquiredAt = Now
        });
        await _postgresFixture.SharedDbContext.SaveChangesAsync();

        // Act
        var (acquired, _) =
            await store.TryAcquireLockAsync(new EventInfo(eventLog.Id, eventLog.CorrelationId), default);

        // Assert
        acquired.Should().BeFalse();
    }

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
