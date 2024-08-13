﻿using System.Diagnostics;
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
using ModularMonolith.Shared.TestUtils.Messaging;
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

    private readonly PostgreSqlContainer _databaseContainer;
    private readonly RabbitMqContainer _messagingContainer;
    private readonly TestBus _testBus = new();
    private readonly TestConsumer<CategoryCreatedEvent> _categoryCreatedConsumer = new();

    private DbContext _dbContext = default!;
    private NpgsqlConnection _connection = default!;
    private WebApplicationFactory<Program> _factory = default!;
    private TestServer _testServer = default!;
    private Respawner _respawner = default!;

    public CategoryManagementFixture()
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
    }

    public async Task InitializeAsync()
    {
        var databaseTask = _databaseContainer.StartAsync();
        var messagingTask = _messagingContainer.StartAsync();

        await Task.WhenAll(databaseTask, messagingTask);

        var connectionString = _databaseContainer.GetConnectionString();

        _connection = new NpgsqlConnection(connectionString);
        _dbContext =
            new PostgresDbContextFactory().CreateDbContext([connectionString]);

        await _testBus.StartAsync(_messagingContainer.GetConnectionString());
        await _connection.OpenAsync();
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
            builder.UseSetting("ConnectionStrings:RabbitMQ", _messagingContainer.GetConnectionString());
            builder.UseSetting("Messaging:CustomersEnabled", "false");
        });

        _testServer = _factory.Server;
        _testBus.ConnectReceiveEndpoint("CategoryCreatedTestQueue", _categoryCreatedConsumer);
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

    public Task<CategoryCreatedEvent> VerifyCategoryCreatedEventReceived() =>
        new MessagePublicationVerifier<CategoryCreatedEvent>(_categoryCreatedConsumer).VerifyAsync();

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

        return _respawner.ResetAsync(_connection);
    }
}
