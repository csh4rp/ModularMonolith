using Microsoft.EntityFrameworkCore;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using ModularMonolith.CategoryManagement.Migrations;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.IntegrationTests.Common;
using ModularMonolith.Shared.Migrations;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime, IDatabaseFixture
{
    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;
    
    public string ConnectionString => _container!.GetConnectionString();

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

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();
        
        SharedDbContext = new SharedDbContextFactory().CreateDbContext([ConnectionString]);
        await SharedDbContext.Database.MigrateAsync();
        
        CategoryManagementDbContext = new CategoryManagementDbContextFactory().CreateDbContext([ConnectionString]);
        await CategoryManagementDbContext.Database.MigrateAsync();
        
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions { DbAdapter = DbAdapter.Postgres });
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
        
    }
}
