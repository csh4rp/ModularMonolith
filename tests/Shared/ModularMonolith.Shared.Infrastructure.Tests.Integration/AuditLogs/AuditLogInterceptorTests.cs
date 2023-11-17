using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Infrastructure.AuditLogs;
using ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs.Entities;
using ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs.Fixtures;
using NSubstitute;

namespace ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs;

[Collection("Postgres")]
public class AuditLogInterceptorTests : IAsyncDisposable
{
    private static readonly DateTimeOffset Now = new(2023, 11, 3, 15, 30, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.Parse("48A12418-9B7E-471F-930A-CBC7EB390F07");
    private readonly ActivityListener _activityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => {},
        ActivityStopped = _ => {},
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
    };
    
    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly TimeProvider _dateTimeProvider = Substitute.For<TimeProvider>();

    private readonly PostgresFixture _postgresFixture;

    public AuditLogInterceptorTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _dateTimeProvider.GetUtcNow().Returns(Now);
        _identityContextAccessor.Context.Returns(new IdentityContext(UserId));
        
        ActivitySource.AddActivityListener(_activityListener);
    }

    [Fact]
    public async Task ShouldCreateAuditLogForAddedEntity()
    {
        using var activity = new Activity("AddingFirstTestEntity");
        activity.Start();
        await using var cnx = CreateDbContext();
        
        var entity = new FirstTestEntity
        {
            Id = Guid.Parse("4d4d085b-5266-4945-924a-e4177d79c65d"),
            Name = "Name",
            Sensitive = "Value",
            OwnedEntity = new OwnedEntity
            {
                Name = "Entity-Name"
            }
        };

        cnx.FirstTestEntities.Add(entity);
        await cnx.SaveChangesAsync();

        await using var searchCnx = CreateDbContext();

        var auditLog = searchCnx.Set<AuditLog>().SingleOrDefault(a =>
            a.ChangeType == ChangeType.Added && a.EntityType == typeof(FirstTestEntity).FullName);

        auditLog.Should().NotBeNull();
        auditLog!.CreatedAt.Should().Be(Now);
        auditLog.UserId.Should().Be(UserId);
        auditLog.OperationName.Should().Be(activity.OperationName);
        auditLog.TraceId.Should().Be(activity.TraceId.ToString());
        auditLog.EntityKeys.Should().Be(
            """
            {"Id": "4d4d085b-5266-4945-924a-e4177d79c65d"}
            """);
        auditLog.Changes.Should().Be(
            """
            {"Name": {"CurrentValue": "Name", "OriginalValue": null}, "OwnedEntity": {"CurrentValue": {"Name": "Entity-Name"}, "OriginalValue": null}}
            """);
    }

    private TestDbContext CreateDbContext()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AuditLogFactory>();
        serviceCollection.AddTransient<IIdentityContextAccessor>(_ => _identityContextAccessor);
        serviceCollection.AddTransient<TimeProvider>(_ => _dateTimeProvider);
        
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        builder.UseApplicationServiceProvider(serviceCollection.BuildServiceProvider());
        builder.AddInterceptors(new AuditLogInterceptor());
        builder.UseNpgsql(PostgresFixture.ConnectionString);
        
        return new TestDbContext(builder.Options);
    }

    public async ValueTask DisposeAsync() => await _postgresFixture.ResetAsync();
    
}
