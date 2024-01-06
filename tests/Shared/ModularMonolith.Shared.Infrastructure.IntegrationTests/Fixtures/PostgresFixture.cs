using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.DataAccess;
using ModularMonolith.Shared.Migrations;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;
    public SharedDbContext SharedDbContext { get; private set; } = default!;

    public string ConnectionString => _container!.GetConnectionString();

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("shared_automated_tests")
            .WithUsername("testuser")
            .WithPassword("testuser123")
            .WithPortBinding("5432", true)
            .Build();

        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

        SharedDbContext = new SharedDbContextFactory().CreateDbContext([connectionString]);
        await SharedDbContext.Database.MigrateAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE "first_test_entity"
            (
                "id" UUID NOT NULL PRIMARY KEY,
                "name" VARCHAR(128),
                "sensitive" VARCHAR(128),
                "owned_entity" JSONB
            );

            CREATE TABLE "second_test_entity"
            (
                "id" UUID NOT NULL PRIMARY KEY,
                "name" VARCHAR(128)
            );

            CREATE TABLE "first_entity_second_entity"
            (
                "first_test_entity_id" UUID NOT NULL,
                "second_test_entity_id" UUID NOT NULL,
                PRIMARY KEY ("first_test_entity_id", "second_test_entity_id")
            );
            """;

        await cmd.ExecuteNonQueryAsync();

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
