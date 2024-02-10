using ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs;

[Collection("AuditLogs")]
public class AuditLogInterceptorTests : IAsyncDisposable
{
    private readonly PostgresFixture _postgresFixture;
    private readonly AuditLogInterceptorFixture _auditLogInterceptorFixture;

    public AuditLogInterceptorTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _auditLogInterceptorFixture = new AuditLogInterceptorFixture(_postgresFixture.ConnectionString);
    }

    [Fact]
    public async Task ShouldCreateAuditLogs_WhenEntityAndOwnedEntityAreAdded()
    {
        // Arrange
        using var activity = _auditLogInterceptorFixture.StartActivity();

        // Act
        var entity = await _auditLogInterceptorFixture.AddEntityAsync();

        // Assert
        await _auditLogInterceptorFixture.AssertEntityAddedLogWasCreatedAsync(entity);
        await _auditLogInterceptorFixture.AssertOwnedEntityAddedLogWasCreatedAsync(entity.Id, entity.OwnedEntity!);
    }

    [Fact]
    public async Task ShouldCreateAuditLog_WhenOwnedEntityIsUpdated()
    {
        // Arrange
        using var activity = _auditLogInterceptorFixture.StartActivity();
        
        var entity = await _auditLogInterceptorFixture.AddEntityAsync();
        
        // Act
        var originalValue = entity.OwnedEntity!.Name;
        entity.OwnedEntity.Name = "12";

        await _auditLogInterceptorFixture.SaveDbContextChangesAsync();

        // Assert
        await _auditLogInterceptorFixture.AssertEntityModifiedLogWasNotCreatedAsync(entity);
        await _auditLogInterceptorFixture.AssertOwnedEntityModifiedLogWasCreatedAsync(entity.Id,
            [new("Name", entity.OwnedEntity.Name, originalValue)]!);
    }

    public async ValueTask DisposeAsync() => await _postgresFixture.ResetAsync();
}
