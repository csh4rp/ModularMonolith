using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs.Entities;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs;

[Collection("AuditLogs")]
public class AuditLogInterceptorTests : IAsyncLifetime
{
    private readonly SqlServerFixture _sqlServerFixture;
    private readonly AuditLogInterceptorFixture _fixture;
    
    public AuditLogInterceptorTests(SqlServerFixture sqlServerFixture)
    {
        _sqlServerFixture = sqlServerFixture;
        _fixture = new AuditLogInterceptorFixture(_sqlServerFixture.ConnectionString);
    }
    
    [Fact]
    public async Task ShouldNotGenerateLog_WhenEntityIsIgnored()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        var notAuditedEntity = new SecondTestEntity { Id = Guid.NewGuid(), Name = "Test" };

        context.Add(notAuditedEntity);

        // Act
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
        var log = await context.AuditLogs.FirstOrDefaultAsync();

        log.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGenerateLog_WhenEntityIsNotIgnored()
    {
        // Arrange
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

        // Act
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
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
        // Arrange
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

        // Act
        await context.SaveChangesAsync(CancellationToken.None);

        // Assert
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

    public Task DisposeAsync() => _sqlServerFixture.ResetAsync();
}
