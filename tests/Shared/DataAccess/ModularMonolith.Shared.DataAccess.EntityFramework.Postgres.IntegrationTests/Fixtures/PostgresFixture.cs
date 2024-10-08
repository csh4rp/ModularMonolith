﻿using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Infrastructure.Migrations.Postgres;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;

    public DbContext DbContext { get; private set; } = default!;

    public string ConnectionString => _container!.GetConnectionString();

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16.1")
            .Build();

        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

        DbContext = new PostgresDbContextFactory().CreateDbContext([connectionString]);
        await DbContext.Database.MigrateAsync();

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
