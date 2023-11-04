using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.BusinessLogic.Identity;
using ModularMonolith.Shared.Core;
using ModularMonolith.Shared.Infrastructure.AuditLogs;
using ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs.Entities;
using ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs.Fixtures;
using NSubstitute;

namespace ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs;

[Collection("Postgres")]
public class AuditLogInterceptorTests : IAsyncDisposable
{
    private readonly ActivityListener _activityListener = new()
    {
        ShouldListenTo = f => true,
        ActivityStarted = activity => {},
        ActivityStopped = activity => {},
        Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
    };
    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly PostgresFixture _postgresFixture;

    public AuditLogInterceptorTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        
        ActivitySource.AddActivityListener(_activityListener);
    }

    [Fact]
#pragma warning disable VSTHRD200
    public async Task ShouldCreateAuditLog()
#pragma warning restore VSTHRD200
    {
        var ac = new Activity("AddingFirstTestEntity");
        ac.Start();
        await using var cnx = CreateDbContext();

        var entity = new FirstTestEntity { Id = Guid.NewGuid(), Name = "Name", Sensitive = "Value" };

        cnx.FirstTestEntities.Add(entity);
        await cnx.SaveChangesAsync();
    }

    private TestDbContext CreateDbContext()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AuditLogFactory>();
        serviceCollection.AddTransient<IIdentityContextAccessor>(_ => _identityContextAccessor);
        serviceCollection.AddTransient<IDateTimeProvider>(_ => _dateTimeProvider);
        
        var builder = new DbContextOptionsBuilder<TestDbContext>();
        builder.UseApplicationServiceProvider(serviceCollection.BuildServiceProvider());
        builder.AddInterceptors(new AuditLogInterceptor());
        builder.UseNpgsql(_postgresFixture.ConnectionString);
        
        return new TestDbContext(builder.Options);
    }

    public async ValueTask DisposeAsync() => await _postgresFixture.ResetAsync();
    
}
