using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.AuditTrail;
using ModularMonolith.Shared.AuditTrail.EntityFramework.Factories;
using ModularMonolith.Shared.AuditTrail.EntityFramework.Interceptors;
using ModularMonolith.Shared.Identity;
using ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs.Entities;
using NSubstitute;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs;

public class AuditLogInterceptorFixture
{
    private const string UserName = "mail@mail.com";

    private static readonly DateTimeOffset Now = new(2023, 11, 3, 15, 30, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.Parse("018C4AA8-3842-43D9-B0C5-236D442787D5");

    private static readonly ActivityListener ActivityListener = new()
    {
        ShouldListenTo = _ => true,
        ActivityStarted = _ => { },
        ActivityStopped = _ => { },
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
    };

    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly TimeProvider _dateTimeProvider = Substitute.For<TimeProvider>();

    private readonly DbContext _dbContext;
    private Activity? _activity;

    public AuditLogInterceptorFixture(string connectionString)
    {
        _dbContext = CreateDbContext(connectionString);
        _dateTimeProvider.GetUtcNow().Returns(Now);
        _identityContextAccessor.IdentityContext.Returns(new IdentityContext(UserName));
        ActivitySource.AddActivityListener(ActivityListener);
    }

    public Activity StartActivity()
    {
        _activity = new Activity("SampleActivity");
        _activity.Start();

        return _activity;
    }

    public async Task<FirstTestEntity> AddEntityAsync()
    {
        var entity = new FirstTestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Fancy name",
            Sensitive = "Value",
            OwnedEntity = new OwnedEntity { Name = "Entity-Name" }
        };

        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public async Task AssertEntityAddedLogWasCreatedAsync(FirstTestEntity entity)
    {
        var auditLog = await _dbContext.Set<AuditLog>().SingleOrDefaultAsync(a =>
            a.EntityState == AuditTrail.EntityState.Added
            && a.EntityType == typeof(FirstTestEntity).FullName
            && a.EntityKeys.Any(k => k.Value == entity.Id.ToString()));

        auditLog.Should().NotBeNull();
        auditLog!.CreatedAt.Should().Be(Now);
        auditLog.MetaData.Subject.Should().Be(UserName);
        auditLog.MetaData.OperationName.Should().Be(_activity!.OperationName);
        auditLog.MetaData.TraceId.Should().Be(_activity.TraceId.ToString());
        auditLog.EntityKeys.Should().BeEquivalentTo(new List<EntityKey>
        {
            new("Id", entity.Id.ToString())
        });
        auditLog.EntityPropertyChanges.Should().BeEquivalentTo(new List<PropertyChange>
        {
            new("Name", entity.Name, null)
        });
    }

    public async Task AssertEntityModifiedLogWasNotCreatedAsync(FirstTestEntity entity)
    {
        var auditLog = await _dbContext.Set<AuditLog>().SingleOrDefaultAsync(a =>
            a.EntityState == AuditTrail.EntityState.Modified
            && a.EntityType == typeof(FirstTestEntity).FullName
            && a.EntityKeys.Any(k => k.Value == entity.Id.ToString()));

        auditLog.Should().BeNull();
    }

    public async Task AssertOwnedEntityAddedLogWasCreatedAsync(Guid parentEntityId, OwnedEntity ownedEntity)
    {
        var ownedEntityAuditLog = await _dbContext.Set<AuditLog>().SingleOrDefaultAsync(a =>
            a.EntityState == AuditTrail.EntityState.Added
            && a.EntityType == typeof(OwnedEntity).FullName
            && a.EntityKeys.Any(k => k.Value == parentEntityId.ToString()));

        ownedEntityAuditLog.Should().NotBeNull();
        ownedEntityAuditLog!.CreatedAt.Should().Be(Now);
        ownedEntityAuditLog.MetaData.Subject.Should().Be(UserName);
        ownedEntityAuditLog.MetaData.OperationName.Should().Be(_activity!.OperationName);
        ownedEntityAuditLog.MetaData.TraceId.Should().Be(_activity.TraceId.ToString());
        ownedEntityAuditLog.EntityKeys.Should().BeEquivalentTo(new List<EntityKey>
        {
            new("OwnerEntityId", parentEntityId.ToString())
        });
        ownedEntityAuditLog.EntityPropertyChanges.Should().BeEquivalentTo(new List<PropertyChange>
        {
            new("Name", ownedEntity.Name, null)
        });
    }

    public async Task AssertOwnedEntityModifiedLogWasCreatedAsync(Guid parentEntityId, List<PropertyChange> expectedChanges)
    {
        var ownedEntityAuditLog = await _dbContext.Set<AuditLog>().SingleOrDefaultAsync(a =>
            a.EntityState == AuditTrail.EntityState.Modified
            && a.EntityType == typeof(OwnedEntity).FullName
            && a.EntityKeys.Any(k => k.Value == parentEntityId.ToString()));

        ownedEntityAuditLog.Should().NotBeNull();
        ownedEntityAuditLog!.CreatedAt.Should().Be(Now);
        ownedEntityAuditLog.MetaData.Subject.Should().Be(UserName);
        ownedEntityAuditLog.MetaData.OperationName.Should().Be(_activity!.OperationName);
        ownedEntityAuditLog.MetaData.TraceId.Should().Be(_activity.TraceId.ToString());
        ownedEntityAuditLog.EntityKeys.Should().BeEquivalentTo(new List<EntityKey>
        {
            new("OwnerEntityId", parentEntityId.ToString())
        });
        ownedEntityAuditLog.EntityPropertyChanges.Should().BeEquivalentTo(expectedChanges);
    }

    public Task SaveDbContextChangesAsync() => _dbContext.SaveChangesAsync();

    private AuditLogTestDbContext CreateDbContext(string connectionString)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AuditLogFactory>();
        serviceCollection.AddTransient<IIdentityContextAccessor>(_ => _identityContextAccessor);
        serviceCollection.AddTransient<IHttpContextAccessor>(_ =>
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        serviceCollection.AddTransient<TimeProvider>(_ => _dateTimeProvider);

        var builder = new DbContextOptionsBuilder<AuditLogTestDbContext>();
        builder.UseApplicationServiceProvider(serviceCollection.BuildServiceProvider());
        builder.AddInterceptors(new AuditLogInterceptor());
        builder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new AuditLogTestDbContext(builder.Options);
    }
}
