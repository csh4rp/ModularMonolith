using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.Application.Identity;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Domain.ValueObjects;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Factories;
using ModularMonolith.Shared.Infrastructure.AuditLogs.Interceptors;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs.Entities;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs.Fixtures;
using NSubstitute;
using EntityState = ModularMonolith.Shared.Domain.Enums.EntityState;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs;

[Collection("Postgres")]
public class AuditLogInterceptorTests : IAsyncDisposable
{
    private static readonly DateTimeOffset Now = new(2023, 11, 3, 15, 30, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.Parse("018C4AA8-3842-43D9-B0C5-236D442787D5");
    private static readonly string UserName = "mail@mail.com";

    private readonly ActivityListener _activityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => { },
        ActivityStopped = _ => { },
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };

    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly TimeProvider _dateTimeProvider = Substitute.For<TimeProvider>();

    private readonly PostgresFixture _postgresFixture;

    public AuditLogInterceptorTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _dateTimeProvider.GetUtcNow().Returns(Now);
        _identityContextAccessor.Context.Returns(new IdentityContext(UserId, UserName));

        ActivitySource.AddActivityListener(_activityListener);
    }

    [Fact]
    public async Task ShouldCreateAuditLogs()
    {
        // Arrange
        using var activity = new Activity("AddingFirstTestEntity");
        activity.Start();
        await using var cnx = CreateDbContext();

        var entity = new FirstTestEntity
        {
            Id = Guid.Parse("4d4d085b-5266-4945-924a-e4177d79c65d"),
            Name = "Fancy name",
            Sensitive = "Value",
            OwnedEntity = new OwnedEntity { Name = "Entity-Name" }
        };

        // Act
        cnx.FirstTestEntities.Add(entity);
        await cnx.SaveChangesAsync();

        // Assert
        await using var searchCnx = CreateDbContext();

        var parentEntityAuditLog = searchCnx.Set<AuditLog>().SingleOrDefault(a =>
            a.EntityState == EntityState.Added
            && a.EntityType == typeof(FirstTestEntity).FullName
            && a.EntityKeys.Any(k => k.Value == entity.Id.ToString()));

        parentEntityAuditLog.Should().NotBeNull();
        parentEntityAuditLog!.CreatedAt.Should().Be(Now);
        parentEntityAuditLog.UserName.Should().Be(UserName);
        parentEntityAuditLog.OperationName.Should().Be(activity.OperationName);
        parentEntityAuditLog.TraceId.Should().Be(activity.TraceId.ToString());
        parentEntityAuditLog.EntityKeys.Should().BeEquivalentTo(new List<EntityKey>
        {
            new("Id", "4d4d085b-5266-4945-924a-e4177d79c65d")
        });
        parentEntityAuditLog.EntityPropertyChanges.Should().BeEquivalentTo(new List<PropertyChange>
        {
            new("Name", "Fancy name", null)
        });

        var ownedEntityAuditLog = searchCnx.Set<AuditLog>().SingleOrDefault(a =>
            a.EntityState == EntityState.Added
            && a.EntityType == typeof(OwnedEntity).FullName
            && a.EntityKeys.Any(k => k.Value == entity.Id.ToString()));

        ownedEntityAuditLog.Should().NotBeNull();
        ownedEntityAuditLog!.CreatedAt.Should().Be(Now);
        ownedEntityAuditLog.UserName.Should().Be(UserName);
        ownedEntityAuditLog.OperationName.Should().Be(activity.OperationName);
        ownedEntityAuditLog.TraceId.Should().Be(activity.TraceId.ToString());
        ownedEntityAuditLog.EntityKeys.Should().BeEquivalentTo(new List<EntityKey>
        {
            new("OwnerEntityId", "4d4d085b-5266-4945-924a-e4177d79c65d")
        });
        ownedEntityAuditLog.EntityPropertyChanges.Should().BeEquivalentTo(new List<PropertyChange>
        {
            new("Name", "Entity-Name", null)
        });
    }

    [Fact]
    public async Task ShouldCreateAuditLog_WhenOwnedEntityIsUpdated()
    {
        // Arrange
        using var activity = new Activity("AddingFirstTestEntity");
        activity.Start();
        await using var cnx = CreateDbContext();

        var entity = new FirstTestEntity
        {
            Id = Guid.Parse("5A9665C9-5B96-4DC6-8D05-5A33D91DE88F"),
            Name = "Name",
            Sensitive = "Value",
            OwnedEntity = new OwnedEntity { Name = "Entity-Name" }
        };

        // Act
        cnx.FirstTestEntities.Add(entity);
        await cnx.SaveChangesAsync();

        entity.OwnedEntity.Name = "12";

        await cnx.SaveChangesAsync();

        // Assert
        await using var searchCnx = CreateDbContext();

        var parentEntityAuditLog = searchCnx.Set<AuditLog>().SingleOrDefault(a =>
            a.EntityState == EntityState.Modified
            && a.EntityType == typeof(FirstTestEntity).FullName
            && a.EntityKeys.Any(k => k.Value == entity.Id.ToString()));

        parentEntityAuditLog.Should().BeNull();

        var ownedEntityAuditLog = searchCnx.Set<AuditLog>().SingleOrDefault(a =>
            a.EntityState == EntityState.Modified
            && a.EntityType == typeof(OwnedEntity).FullName
            && a.EntityKeys.Any(k => k.Value == entity.Id.ToString()));

        ownedEntityAuditLog.Should().NotBeNull();
        ownedEntityAuditLog!.CreatedAt.Should().Be(Now);
        ownedEntityAuditLog.UserName.Should().Be(UserName);
        ownedEntityAuditLog.OperationName.Should().Be(activity.OperationName);
        ownedEntityAuditLog.TraceId.Should().Be(activity.TraceId.ToString());
        ownedEntityAuditLog.EntityKeys.Should().BeEquivalentTo(new List<EntityKey>
        {
            new("OwnerEntityId", "5a9665c9-5b96-4dc6-8d05-5a33d91de88f")
        });
        ownedEntityAuditLog.EntityPropertyChanges.Should().BeEquivalentTo(new List<PropertyChange>
        {
            new("Name", "12", "Entity-Name")
        });
    }

    private TestDbContext CreateDbContext()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AuditLogFactory>();
        serviceCollection.AddTransient<IIdentityContextAccessor>(_ => _identityContextAccessor);
        serviceCollection.AddTransient<IHttpContextAccessor>(_ =>
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        serviceCollection.AddTransient<TimeProvider>(_ => _dateTimeProvider);

        var builder = new DbContextOptionsBuilder<TestDbContext>();
        builder.UseApplicationServiceProvider(serviceCollection.BuildServiceProvider());
        builder.AddInterceptors(new AuditLogInterceptor());
        builder.UseNpgsql(_postgresFixture.ConnectionString)
            .UseSnakeCaseNamingConvention();

        return new TestDbContext(builder.Options);
    }

    public async ValueTask DisposeAsync() => await _postgresFixture.ResetAsync();
}
