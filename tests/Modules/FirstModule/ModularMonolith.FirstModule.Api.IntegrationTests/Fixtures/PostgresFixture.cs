using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Infrastructure.Common.DataAccess;
using ModularMonolith.FirstModule.Migrations;
using ModularMonolith.Shared.Migrations;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.FirstModule.Api.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    public readonly string ConnectionString =
        "Server=localhost; Port=5434; UserName=postgres; Password=Admin123!@#; Database=test_db2;";

    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("test_db2")
            .WithUsername("postgres")
            .WithPassword("Admin123!@#")
            .WithPortBinding("5434", "5432")
            .Build();

        await _container.StartAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();
        
        await using var sharedDbContext = new SharedDbContextFactory().CreateDbContext([ConnectionString]);
        await sharedDbContext.Database.EnsureCreatedAsync();
        // await sharedDbContext.Database.MigrateAsync();
        
        await using var firstModuleDbContext = new FirstModuleDbContextFactory().CreateDbContext([ConnectionString]);//new FirstModuleDbContext(firstModuleOptionsBuilder.Options);
        await firstModuleDbContext.Database.EnsureCreatedAsync();
        await firstModuleDbContext.Database.MigrateAsync();
        
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
    }

    public Task ResetAsync()
    {
        Debug.Assert(_connection is not null);
        Debug.Assert(_respawner is not null);

        return _respawner.ResetAsync(_connection);
    }
}
