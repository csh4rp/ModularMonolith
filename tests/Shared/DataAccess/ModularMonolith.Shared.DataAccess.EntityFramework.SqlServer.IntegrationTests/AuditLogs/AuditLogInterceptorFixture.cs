using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Shared.DataAccess.AudiLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Factories;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs.Interceptors;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.Stores;
using ModularMonolith.Shared.Identity;
using NSubstitute;
using QueryCollection = Microsoft.AspNetCore.Http.QueryCollection;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.AuditLogs;

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
         serviceCollection.AddTransient<SqlServer.AuditLogs.Factories.AuditLogFactory>();
         serviceCollection.AddTransient<IAuditMetaDataProvider, AuditMetaDataProvider>();
         serviceCollection.AddTransient<IIdentityContextAccessor>(_ => _identityContextAccessor);
         serviceCollection.AddTransient<IAuditLogStore, AuditLogStore>();
         serviceCollection.AddTransient<TimeProvider>(_ => _dateTimeProvider);
         serviceCollection.AddTransient<DbContext>(_ => _dbContext!);
         serviceCollection.AddTransient<IHttpContextAccessor>(_ =>
             new HttpContextAccessor { HttpContext = new DefaultHttpContext()
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
             } });

         var builder = new DbContextOptionsBuilder<AuditLogDbContext>();
         builder.UseApplicationServiceProvider(serviceCollection.BuildServiceProvider());
         builder.AddInterceptors(new AuditLogInterceptor());
         builder.UseSqlServer(_connectionString);

         _dbContext = new AuditLogDbContext(builder.Options);
         return _dbContext;
     }

     public async Task InitializeAsync()
     {
         await using var connection = new SqlConnection(_connectionString);
         await connection.OpenAsync();

         await using var cmd = connection.CreateCommand();
         cmd.CommandText =
             """
             IF NOT EXISTS (SELECT 1 FROM SYSOBJECTS WHERE NAME = 'FirstTestEntity' AND XTYPE = 'U')
                 CREATE TABLE FirstTestEntity
                 (
                     Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                     Timestamp DATETIMEOFFSET  NOT NULL,
                     Name NVARCHAR(128) NOT NULL,
                     FirstOwnedEntity NVARCHAR(MAX) NULL,
                     SecondOwnedEntity NVARCHAR(MAX) NULL
                 );

             IF NOT EXISTS (SELECT 1 FROM SYSOBJECTS WHERE NAME = 'SecondTestEntity' AND XTYPE = 'U')
                CREATE TABLE SecondTestEntity
                (
                    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    Name NVARCHAR(128) NOT NULL
                );


             IF NOT EXISTS (SELECT 1 FROM SYSOBJECTS WHERE NAME = 'FirstSecondTestEntity' AND XTYPE = 'U')
                CREATE TABLE  FirstSecondTestEntity
                (
                    FirstTestEntityId UNIQUEIDENTIFIER NOT NULL,
                    SecondTestEntityId UNIQUEIDENTIFIER NOT NULL,
                    PRIMARY KEY (FirstTestEntityId, SecondTestEntityId)
                );
             """;

         await cmd.ExecuteNonQueryAsync();
     }

     public Task DisposeAsync()
     {
         return Task.CompletedTask;
     }
}
