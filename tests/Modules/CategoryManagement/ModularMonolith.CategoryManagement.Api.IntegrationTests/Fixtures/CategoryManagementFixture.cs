using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.CategoryManagement.Migrations;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Migrations;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;

public class CategoryManagementFixture : IAsyncLifetime
{
    private const string AuthAudience = "localhost";
    private const string AuthIssuer = "localhost";
    private const string AuthSigningKey = "12345678123456781234567812345678";
    
    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;
    private WebApplicationFactory<Program> _factory = default!;
    private TestServer _testServer = default!;
    
    public SharedDbContext SharedDbContext { get; private set; } = default!;
    
    public CategoryManagementDbContext CategoryManagementDbContext { get; private set; } = default!;
    
    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16.1")
            .WithName("category_management_automated_tests")
            .WithDatabase("tests_database")
            .WithUsername("postgres")
            .WithPassword("Admin123!@#")
            .WithPortBinding("5432", true)
            .Build();
        
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();
        
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();
        
        SharedDbContext = new SharedDbContextFactory().CreateDbContext([connectionString]);
        await SharedDbContext.Database.MigrateAsync();
        
        CategoryManagementDbContext = new CategoryManagementDbContextFactory().CreateDbContext([connectionString]);
        await CategoryManagementDbContext.Database.MigrateAsync();
        
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", _container!.GetConnectionString());
            builder.UseSetting("Modules:CategoryManagement:Enabled", "true");
            builder.UseSetting("Authentication:Type", "Bearer");
            builder.UseSetting("Authentication:Audience", AuthAudience);
            builder.UseSetting("Authentication:Issuer", AuthIssuer);
            builder.UseSetting("Authentication:SigningKey", AuthSigningKey);
            builder.UseSetting("Logging:LogLevel:Default", "Warning");
        });

        _testServer = _factory.Server;
    }
    
    public HttpClient CreateClient()
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
        var claims = new[]
        {
            new Claim("id", Guid.NewGuid().ToString()),
            new Claim("sub","mail@mail.com"),
        };
        
        var token = new JwtSecurityToken(AuthIssuer,
            AuthAudience,
            claims,
            expires: DateTimeOffset.UtcNow.AddMinutes(15).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);        
    }
    
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

        await SharedDbContext.DisposeAsync();
        await CategoryManagementDbContext.DisposeAsync();
        await _factory.DisposeAsync();
    }
    
    public Task ResetAsync()
    {
        Debug.Assert(_connection is not null);
        Debug.Assert(_respawner is not null);

        return _respawner.ResetAsync(_connection);
    }
}
