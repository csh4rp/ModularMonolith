using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Interceptors;
using ModularMonolith.Shared.Identity;
using Npgsql;
using NSubstitute;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.AuditLogs;

public class AuditLogInterceptorFixture : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    
    private readonly TimeProvider _dateTimeProvider = Substitute.For<TimeProvider>();
    private readonly IIdentityContextAccessor _identityContextAccessor = Substitute.For<IIdentityContextAccessor>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    
    private readonly string _connectionString;
    
    public AuditLogInterceptorFixture(string connectionString)
    {
        _connectionString = connectionString;
        _dateTimeProvider.GetUtcNow().Returns(Now);
    }

     public AuditLogDbContext CreateDbContext()
     {
         var serviceCollection = new ServiceCollection();
         serviceCollection.AddTransient<IHttpContextAccessor>(_ => _httpContextAccessor);
         serviceCollection.AddTransient<AuditLogFactory>();
         serviceCollection.AddTransient<IAuditMetaDataProvider, AuditMetaDataProvider>();
         serviceCollection.AddTransient<IIdentityContextAccessor>(_ => _identityContextAccessor);
         serviceCollection.AddTransient<IHttpContextAccessor>(_ =>
             new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
         serviceCollection.AddTransient<TimeProvider>(_ => _dateTimeProvider);

         var builder = new DbContextOptionsBuilder<AuditLogDbContext>();
         builder.UseApplicationServiceProvider(serviceCollection.BuildServiceProvider());
         builder.AddInterceptors(new AuditLogInterceptor());
         builder.UseNpgsql(_connectionString)
             .UseSnakeCaseNamingConvention();

         return new AuditLogDbContext(builder.Options);
     }

     public async Task InitializeAsync()
     {
         await using var connection = new NpgsqlConnection(_connectionString);
         await connection.OpenAsync();
         
         await using var cmd = connection.CreateCommand();
         cmd.CommandText =
             """
             CREATE TABLE "FirstTestEntity"
             (
                 "id" UUID NOT NULL PRIMARY KEY,
                 "timestamp" TIMESTAMP NOT NULL,
                 "name" VARCHAR(128) NOT NULL,
                 "owned_entity" JSONB
             );

             CREATE TABLE "SecondTestEntity"
             (
                 "id" UUID NOT NULL PRIMARY KEY,
                 "name" VARCHAR(128) NOT NULL
             );

             CREATE TABLE "FirstSecondTestEntity"
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
