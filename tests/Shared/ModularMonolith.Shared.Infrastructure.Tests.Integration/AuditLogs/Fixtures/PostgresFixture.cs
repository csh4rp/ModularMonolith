using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.Shared.Infrastructure.Tests.Integration.AuditLogs.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    public readonly string ConnectionString =
        "Server=localhost; Port=5434; UserName=testuser; Password=testuser123; Database=test_db;";

    private NpgsqlConnection _connection = default!;
    private PostgreSqlContainer _container = default!;
    private Respawner _respawner = default!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("testuser")
            .WithPassword("testuser123")
            .WithPortBinding("5434", "5432")
            .Build();

        await _container.StartAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = 
            """
            CREATE TABLE FirstTestEntity
            (
                Id UUID NOT NULL PRIMARY KEY,
                Name VARCHAR(128),
                Sensitive VARCHAR(128),
                OwnedEntity JSONB
            );

            CREATE TABLE SecondTestEntity
            (
                Id UUID NOT NULL PRIMARY KEY,
                Name VARCHAR(128)
            );

            CREATE TABLE FirstEntitySecondEntity
            (
                FirstTestEntityId UUID NOT NULL,
                SecondTestEntityId UUID NOT NULL,
                PRIMARY KEY (FirstTestEntityId, SecondTestEntityID)
            );

            CREATE TABLE AuditLog
            (
                Id UUID NOT NULL PRIMARY KEY,
                CreatedAt TIMESTAMP NOT NULL,
                ChangeType VARCHAR(8) NOT NULL,
                EntityType VARCHAR(255) NOT NULL,
                EntityKeys JSONB NOT NULL,
                Changes JSONB NOT NULL,
                UserId UUID,
                OperationName VARCHAR(128),
                TraceId VARCHAR(32)
            );
            """;

        await cmd.ExecuteNonQueryAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
        });
    }

    public async Task DisposeAsync()
    {
        await _respawner.ResetAsync(_connection);
        await _connection.DisposeAsync();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    public Task ResetAsync()
    {
        return _respawner.ResetAsync(_connection);
    }
}
