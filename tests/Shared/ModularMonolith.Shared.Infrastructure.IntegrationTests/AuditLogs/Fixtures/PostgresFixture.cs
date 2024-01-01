using System.Diagnostics;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.Shared.Infrastructure.IntegrationTests.AuditLogs.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;

    public string ConnectionString => _container!.GetConnectionString();

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithName("shared_automated_tests")
            .WithUsername("testuser")
            .WithPassword("testuser123")
            .WithPortBinding("5432", true)
            .Build();

        await _container.StartAsync();

        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

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

            CREATE TABLE "audit_log"
            (
                "id" UUID NOT NULL PRIMARY KEY,
                "created_at" TIMESTAMP WITH TIME ZONE NOT NULL,
                "entity_state" VARCHAR(8) NOT NULL,
                "entity_type" VARCHAR(255) NOT NULL,
                "entity_keys" JSONB NOT NULL,
                "entity_property_changes" JSONB NOT NULL,
                "user_id" UUID,
                "user_name" VARCHAR(128),
                "operation_name" VARCHAR(128),
                "trace_id" VARCHAR(32),
                "span_id" VARCHAR(32),
                "parent_span_id" VARCHAR(32),
                "ip_address" VARCHAR(32),
                "user_agent" VARCHAR(32)
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
