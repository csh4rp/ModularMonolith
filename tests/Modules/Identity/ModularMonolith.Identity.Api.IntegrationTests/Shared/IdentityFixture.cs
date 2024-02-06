using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.Bootstrapper.Infrastructure;
using ModularMonolith.Infrastructure.Migrations;
using ModularMonolith.Shared.TestUtils.Fakes;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.Identity.Api.IntegrationTests.Shared;

public class IdentityFixture : IAsyncLifetime
{
    private const string AuthAudience = "localhost";
    private const string AuthIssuer = "localhost";
    private const string AuthSigningKey = "12345678123456781234567812345678";

    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;
    private WebApplicationFactory<Program> _factory = default!;
    private TestServer _testServer = default!;

    public ApplicationDbContext DbContext { get; private set; } = default!;
    
    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16.1")
            .WithName("identity_automated_tests")
            .WithDatabase("tests_database")
            .Build();

        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();

        DbContext = new ApplicationDbContextFactory().CreateDbContext([connectionString]);
        await DbContext.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", connectionString);
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

            builder.ConfigureServices(s =>
            {
                s.Replace(new ServiceDescriptor(typeof(TimeProvider), typeof(FakeTimeProvider),
                    ServiceLifetime.Singleton));
            });
        });

        _testServer = _factory.Server;
    }

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

    public AsyncServiceScope CreateServiceScope() => _testServer.Services.CreateAsyncScope();

    public async Task DisposeAsync()
    {
        if (_respawner is not null && _connection is not null)
        {
            await _respawner.ResetAsync(_connection);
            await _connection.DisposeAsync();
        }

        if (_container is not null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }

        await DbContext.DisposeAsync();
        await _factory.DisposeAsync();
    }

    public Task ResetAsync()
    {
        Debug.Assert(_connection is not null);
        Debug.Assert(_respawner is not null);

        DbContext.ChangeTracker.Clear();
        return _respawner.ResetAsync(_connection);
    }
}
