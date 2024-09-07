using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Infrastructure.Migrations.SqlServer;
using ModularMonolith.Tests.Utils.SqlServer;
using Respawn;
using Testcontainers.MsSql;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.IntegrationTests.Fixtures;

public class SqlServerFixture : IAsyncLifetime
{
    private SqlConnection? _connection;
    private MsSqlContainer? _container;
    private Respawner? _respawner;

    public DbContext DbContext { get; private set; } = default!;

    public string ConnectionString => _container!.GetConnectionString();

    public async Task InitializeAsync()
    {
        _container = new SqlServerContainerBuilder()
            .Build();

        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        _connection = new SqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

        DbContext = new SqlServerDbContextFactory().CreateDbContext([connectionString]);
        await DbContext.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions { DbAdapter = DbAdapter.SqlServer });
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
