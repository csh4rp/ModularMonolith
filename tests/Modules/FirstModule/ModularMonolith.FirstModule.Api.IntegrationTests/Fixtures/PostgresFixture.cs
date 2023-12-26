﻿using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.FirstModule.Infrastructure.Common.DataAccess;
using ModularMonolith.FirstModule.Migrations;
using ModularMonolith.Shared.Infrastructure.DataAccess.Internal;
using ModularMonolith.Shared.Migrations;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace ModularMonolith.FirstModule.Api.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private NpgsqlConnection? _connection;
    private PostgreSqlContainer? _container;
    private Respawner? _respawner;
    
    public string ConnectionString => _container!.GetConnectionString();

    public SharedDbContext SharedDbContext { get; private set; } = default!;
    
    public FirstModuleDbContext FirstModuleDbContext { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16.1")
            .WithName("postgres_automated_tests")
            .WithDatabase("tests_database")
            .WithUsername("postgres")
            .WithPassword("Admin123!@#")
            .WithPortBinding("5434", "5432")
            .Build();
        
        await _container.StartAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();
        
        SharedDbContext = new SharedDbContextFactory().CreateDbContext([ConnectionString]);
        // await SharedDbContext.Database.EnsureCreatedAsync();
        await SharedDbContext.Database.MigrateAsync();
        
        FirstModuleDbContext = new FirstModuleDbContextFactory().CreateDbContext([ConnectionString]);
        // await FirstModuleDbContext.Database.EnsureCreatedAsync();
        await FirstModuleDbContext.Database.MigrateAsync();
        
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
        await FirstModuleDbContext.DisposeAsync();
        
    }

    public Task ResetAsync()
    {
        Debug.Assert(_connection is not null);
        Debug.Assert(_respawner is not null);

        return _respawner.ResetAsync(_connection);
    }
}
