using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Infrastructure.Migrations.SqlServer;
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
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(new SqlServerReadinessCheck()))
            .WithName("shared_infrastructure_integration_tests")
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

    private class SqlServerReadinessCheck : IWaitUntil
    {
        private readonly string[] _command = ["/opt/mssql-tools18/bin/sqlcmd", "-Q", "SELECT 1;", "-C"];

        /// <inheritdoc />
        public async Task<bool> UntilAsync(IContainer container)
        {
            var execResult = await container.ExecAsync(_command)
                .ConfigureAwait(false);

            return 0L.Equals(execResult.ExitCode);
        }
    }
}
