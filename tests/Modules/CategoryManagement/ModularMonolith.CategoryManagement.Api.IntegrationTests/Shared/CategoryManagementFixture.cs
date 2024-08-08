using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.CategoryManagement.Domain.Categories;
using ModularMonolith.CategoryManagement.RestApi;
using ModularMonolith.Infrastructure.Migrations.Postgres;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Shared;

public class CategoryManagementFixture : IAsyncLifetime
{
    private const string AuthAudience = "localhost";
    private const string AuthIssuer = "localhost";
    private const string AuthSigningKey = "12345678123456781234567812345678";

    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _databaseContainer;
    private RabbitMqContainer? _messagingContainer;
    private Respawner? _respawner;
    private WebApplicationFactory<Program> _factory = default!;
    private TestServer _testServer = default!;
    private DbContext _dbContext = default!;

    public async Task InitializeAsync()
    {
        _databaseContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16.1")
            .WithName("category_management_automated_tests")
            .WithDatabase("tests_database")
            .Build();

        _messagingContainer = new RabbitMqBuilder()
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        var databaseTask =  _databaseContainer.StartAsync();
        var messagingTask = _messagingContainer.StartAsync();

        await Task.WhenAll(databaseTask, messagingTask);

        var connectionString = _databaseContainer.GetConnectionString();

        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();

        _dbContext = new PostgresDbContextFactory().CreateDbContext([connectionString, "ModularMonolith.CategoryManagement.Infrastructure"]);
        await _dbContext.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", _databaseContainer!.GetConnectionString());
            builder.UseSetting("DataAccess:Provider", "Postgres");
            builder.UseSetting("Messaging:Provider", "RabbitMQ");
            builder.UseSetting("Modules:CategoryManagement:Enabled", "true");
            builder.UseSetting("Authentication:Type", "Bearer");
            builder.UseSetting("Authentication:Audience", AuthAudience);
            builder.UseSetting("Authentication:Issuer", AuthIssuer);
            builder.UseSetting("Authentication:SigningKey", AuthSigningKey);
            builder.UseSetting("Logging:LogLevel:Default", "Debug");
            builder.UseSetting("Messaging:RabbitMQ:Host", _messagingContainer.Hostname);
            builder.UseSetting("Messaging:RabbitMQ:Username", "guest");
            builder.UseSetting("Messaging:RabbitMQ:Password", "guest");
        });

        _testServer = _factory.Server;
    }

    private HttpClient CreateClient()
    {
        var client = _testServer.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;v=1.0;q=1.0"));

        return client;
    }

    public HttpClient CreateClientWithAuthToken()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken());

        return client;
    }

    private static string GenerateToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthSigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim("id", Guid.NewGuid().ToString()), new Claim("sub", "mail@mail.com") };

        var token = new JwtSecurityToken(AuthIssuer,
            AuthAudience,
            claims,
            expires: DateTimeOffset.UtcNow.AddMinutes(15).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task AddCategoriesAsync(params Category[] categories)
    {
        _dbContext.AddRange(categories);
        await _dbContext.SaveChangesAsync();
    }


    public async Task DisposeAsync()
    {
        if (_respawner is not null && _connection is not null)
        {
            await _respawner.ResetAsync(_connection);
            await _connection.DisposeAsync();
        }

        if (_databaseContainer is not null)
        {
            await _databaseContainer.StopAsync();
            await _databaseContainer.DisposeAsync();
        }

        if (_messagingContainer is not null)
        {
            await _messagingContainer.StartAsync();
            await _messagingContainer.DisposeAsync();
        }

        await _dbContext.DisposeAsync();
        await _factory.DisposeAsync();
    }

    public Task ResetAsync()
    {
        Debug.Assert(_connection is not null);
        Debug.Assert(_respawner is not null);

        return _respawner.ResetAsync(_connection);
    }
}
