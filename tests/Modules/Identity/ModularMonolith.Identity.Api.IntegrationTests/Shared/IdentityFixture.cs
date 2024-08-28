using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.Identity.Domain.Users;
using ModularMonolith.Identity.RestApi;
using ModularMonolith.Infrastructure.Migrations.SqlServer;
using ModularMonolith.Tests.Utils.Fakes;
using ModularMonolith.Tests.Utils.Kafka;
using ModularMonolith.Tests.Utils.SqlServer;
using Respawn;
using Testcontainers.Kafka;
using Testcontainers.MsSql;

namespace ModularMonolith.Identity.Api.IntegrationTests.Shared;

public class IdentityFixture : IAsyncLifetime
{
    private const string AuthAudience = "localhost";
    private const string AuthIssuer = "localhost";
    private const string AuthSigningKey = "12345678123456781234567812345678";

    private readonly MsSqlContainer _databaseContainer;
    private readonly KafkaContainer _messagingContainer;

    private SqlConnection _connection = default!;
    private Respawner _respawner = default!;
    private WebApplicationFactory<Program> _factory = default!;
    private TestServer _testServer = default!;
    private DbContext _dbContext = default!;

    public IdentityFixture()
    {
        _databaseContainer = new SqlServerContainerBuilder().Build();
        _messagingContainer = new KafkaBuilder().Build();
    }

    public async Task InitializeAsync()
    {
        var databaseTask = _databaseContainer.StartAsync();
        var messagingTask = _messagingContainer.StartAsync();

        await Task.WhenAll(databaseTask, messagingTask);

        var connectionString = _databaseContainer.GetConnectionString();

        await _messagingContainer.CreateTopics("PasswordChangedEvent");

        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync();

        _dbContext = new SqlServerDbContextFactory().CreateDbContext([connectionString]);
        await _dbContext.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions { DbAdapter = DbAdapter.SqlServer });

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", connectionString);
            builder.UseSetting("DataAccess:Provider", "SqlServer");
            builder.UseSetting("Messaging:Provider", "Kafka");
            builder.UseSetting("Modules:Identity:Enabled", "true");
            builder.UseSetting("Modules:Identity:Auth:Audience", AuthAudience);
            builder.UseSetting("Modules:Identity:Auth:Issuer", AuthIssuer);
            builder.UseSetting("Modules:Identity:Auth:Key", AuthSigningKey);
            builder.UseSetting("Modules:Identity:Auth:ExpirationTimeInMinutes", "15");
            builder.UseSetting("Authentication:Type", "Bearer");
            builder.UseSetting("Authentication:Audience", AuthAudience);
            builder.UseSetting("Authentication:Issuer", AuthIssuer);
            builder.UseSetting("Authentication:SigningKey", AuthSigningKey);
            builder.UseSetting("Logging:LogLevel:Default", "Warning");
            builder.UseSetting("Events:RunBackgroundWorkers", "false");
            builder.UseSetting("Kafka:Host", _messagingContainer.GetBootstrapAddress());

            builder.ConfigureServices(s =>
            {
                s.Replace(
                    new ServiceDescriptor(typeof(TimeProvider), typeof(FakeTimeProvider), ServiceLifetime.Singleton));
            });
        });

        _testServer = _factory.Server;
    }

    public string GetMessagingConnectionString() => _messagingContainer.GetBootstrapAddress();

    public HttpClient CreateClient()
    {
        var client = _testServer.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;v=1.0;q=1.0"));

        return client;
    }

    public HttpClient CreateClientWithAuthToken(Guid? userId = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken(userId));

        return client;
    }

    private static string GenerateToken(Guid? userId = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthSigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("id", userId.HasValue ? userId.Value.ToString() : Guid.NewGuid().ToString()),
            new Claim("sub", "mail@mail.com")
        };

        var token = new JwtSecurityToken(AuthIssuer,
            AuthAudience,
            claims,
            expires: DateTimeOffset.UtcNow.AddMinutes(15).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task AddUsersAsync(params User[] users)
    {
        _dbContext.AddRange(users);
        return _dbContext.SaveChangesAsync();
    }

    public AsyncServiceScope CreateServiceScope() => _testServer.Services.CreateAsyncScope();

    public async Task DisposeAsync()
    {
        await _respawner.ResetAsync(_connection);
        await _connection.DisposeAsync();

        await _databaseContainer.StopAsync();
        await _databaseContainer.DisposeAsync();

        await _messagingContainer.StartAsync();
        await _messagingContainer.DisposeAsync();

        await _dbContext.DisposeAsync();
        await _factory.DisposeAsync();
    }

    public Task ResetAsync()
    {
        Debug.Assert(_connection is not null);
        Debug.Assert(_respawner is not null);

        _dbContext.ChangeTracker.Clear();
        return _respawner.ResetAsync(_connection);
    }
}
