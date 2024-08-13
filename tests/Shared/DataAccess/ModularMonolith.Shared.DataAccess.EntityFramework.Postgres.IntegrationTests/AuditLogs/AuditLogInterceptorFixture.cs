using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Interceptors;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.Stores;
using ModularMonolith.Shared.Identity;
using Npgsql;
using NSubstitute;
using QueryCollection = Microsoft.AspNetCore.Http.QueryCollection;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs;

public class AuditLogInterceptorFixture : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly ActivitySource ActivitySource = new(nameof(AuditLogInterceptorFixture));


    private readonly TimeProvider _dateTimeProvider = Substitute.For<TimeProvider>();
    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

    private readonly string _connectionString;

    private AuditLogDbContext? _dbContext;

    public AuditLogInterceptorFixture(string connectionString)
    {
        ActivitySource.AddActivityListener(new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
            },
            ActivityStopped = activity =>
            {
            },
        });

        _connectionString = connectionString;
        _dateTimeProvider.GetUtcNow().Returns(Now);
    }

    public Activity StartActivity() => ActivitySource.StartActivity()!;

    public HttpContext GetHttpContext() => _httpContextAccessor.HttpContext!;

    public AuditLogDbContext CreateDbContext()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IHttpContextAccessor>(_ => _httpContextAccessor);
        serviceCollection.AddTransient<AuditLogFactory>();
        serviceCollection.AddTransient<Postgres.AuditLogs.Factories.AuditLogFactory>();
        serviceCollection.AddTransient<IAuditMetaDataProvider, AuditMetaDataProvider>();
        serviceCollection.AddTransient<IIdentityContextAccessor>(_ => _identityContextAccessor);
        serviceCollection.AddTransient<IAuditLogStore, AuditLogStore>();
        serviceCollection.AddTransient<IHttpContextAccessor>(_ =>
            new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
                {
                    Request =
                    {
                        Scheme = "https",
                        Method = "GET",
                        Path = new PathString("/api/test"),
                        Host = new HostString("localhost"),
                        Protocol = "HTTP (1.1)",
                        Query = new QueryCollection()
                    }
                }
            });
        serviceCollection.AddTransient<TimeProvider>(_ => _dateTimeProvider);
        serviceCollection.AddTransient<DbContext>(_ => _dbContext!);

        var builder = new DbContextOptionsBuilder<AuditLogDbContext>();
        builder.UseApplicationServiceProvider(serviceCollection.BuildServiceProvider());
        builder.AddInterceptors(new AuditLogInterceptor());
        builder.UseNpgsql(_connectionString)
            .UseSnakeCaseNamingConvention();

        _dbContext = new AuditLogDbContext(builder.Options);
        return _dbContext;
    }

    public async Task InitializeAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS "FirstTestEntity"
            (
                "id" UUID NOT NULL PRIMARY KEY,
                "timestamp" TIMESTAMP NOT NULL,
                "name" VARCHAR(128) NOT NULL,
                "first_owned_entity" JSONB,
                "second_owned_entity" JSONB
            );

            CREATE TABLE IF NOT EXISTS "SecondTestEntity"
            (
                "id" UUID NOT NULL PRIMARY KEY,
                "name" VARCHAR(128) NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "FirstSecondTestEntity"
            (
                "first_test_entity_id" UUID NOT NULL,
                "second_test_entity_id" UUID NOT NULL,
                PRIMARY KEY ("first_test_entity_id", "second_test_entity_id")
            );
            """;

        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
