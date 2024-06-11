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

        var logs = await context.AuditLogs.FirstOrDefaultAsync();

        logs.Should().BeNull();
    }

    public Task InitializeAsync() => _fixture.InitializeAsync();

    public Task DisposeAsync() => _postgresFixture.ResetAsync();
}
