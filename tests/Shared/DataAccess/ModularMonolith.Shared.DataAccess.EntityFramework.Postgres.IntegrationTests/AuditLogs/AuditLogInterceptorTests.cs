using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs.Entities;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs;

[Collection("AuditLogs")]
public class AuditLogInterceptorTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgresFixture;
    private readonly AuditLogInterceptorFixture _fixture;
    
    public AuditLogInterceptorTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _fixture = new AuditLogInterceptorFixture(_postgresFixture.ConnectionString);
    }
    
    
    [Fact]
    public async Task ShouldNotGenerateLog_WhenEntityIsIgnored()
    {
        await using var context = _fixture.CreateDbContext();

        var notAuditedEntity = new SecondTestEntity { Id = Guid.NewGuid(), Name = "Test" };

        context.Add(notAuditedEntity);

        await context.SaveChangesAsync(CancellationToken.None);

        var log = await context.AuditLogs.FirstOrDefaultAsync();

        log.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGenerateLog_WhenEntityIsNotIgnored()
    {
        using var activity = _fixture.StartActivity();
        await using var context = _fixture.CreateDbContext();

        var auditedEntity = new FirstTestEntity
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTimeOffset.UtcNow,
            Name = "Name_1",
            FirstOwnedEntity = new FirstOwnedEntity { Value = 1.1 },
            SecondOwnedEntity = new SecondOwnedEntity{Value = "1"}
        };

        context.Add(auditedEntity);

        await context.SaveChangesAsync(CancellationToken.None);

        var log = await context.AuditLogs
            .Where(a => a.EntityTypeName == typeof(FirstTestEntity).FullName)
            .FirstOrDefaultAsync();

        log.Should().NotBeNull();
        log!.EntityTypeName.Should().Be(typeof(FirstTestEntity).FullName);
        
        log.EntityKey.Should().HaveCount(1);
        log.EntityKey[0].Name.Should().Be(nameof(FirstTestEntity.Id));
        
        log.EntityChanges.Should().HaveCount(1);
        log.EntityChanges[0].Name.Should().Be(nameof(FirstTestEntity.Name));
        log.EntityChanges[0].CurrentValue.Should().Be(auditedEntity.Name);
        log.EntityChanges[0].OriginalValue.Should().BeNull();

        log.MetaData.Subject.Should().BeNull();
        log.MetaData.OperationName.Should().Be(activity.OperationName);
        log.MetaData.TraceId.Should().Be(activity.TraceId.ToString());
        log.MetaData.SpanId.Should().Be(activity.SpanId.ToString());
        log.MetaData.ParentSpanId.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGenerateLogForOwnedEntity_WhenEntityIsNotIgnored()
    {
        using var activity = _fixture.StartActivity();
        await using var context = _fixture.CreateDbContext();

        var auditedEntity = new FirstTestEntity
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTimeOffset.UtcNow,
            Name = "Name_1",
            FirstOwnedEntity = new FirstOwnedEntity { Value = 1.1 },
            SecondOwnedEntity = new SecondOwnedEntity{Value = "1"}
        };

        context.Add(auditedEntity);

        await context.SaveChangesAsync(CancellationToken.None);

        var log = await context.AuditLogs
            .Where(a => a.EntityTypeName == typeof(SecondOwnedEntity).FullName)
            .FirstOrDefaultAsync();

        log.Should().NotBeNull();
        
        log!.EntityKey.Should().HaveCount(1);
        log.EntityKey[0].Value.Should().Be(auditedEntity.Id.ToString());
        
        log.EntityChanges.Should().HaveCount(1);
        log.EntityChanges[0].Name.Should().Be(nameof(SecondOwnedEntity.Value));
        log.EntityChanges[0].CurrentValue.Should().Be(auditedEntity.SecondOwnedEntity.Value);
        log.EntityChanges[0].OriginalValue.Should().BeNull();
    }

    public Task InitializeAsync() => _fixture.InitializeAsync();

    public Task DisposeAsync() => _postgresFixture.ResetAsync();
}
