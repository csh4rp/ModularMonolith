﻿using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.EntityConfigurations;

namespace ModularMonolith.Infrastructure.DataAccess.Postgres;

public sealed class PostgresDbContext : DbContext
{
    private const string SharedSchemaName = "shared";

    public PostgresDbContext(DbContextOptions<DbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventLogEntityTypeConfiguration(schemaName: SharedSchemaName))
            .ApplyConfiguration(new AuditLogEntityTypeConfiguration(schemaName: SharedSchemaName));

        modelBuilder.AddInboxStateEntity(c =>
        {
            c.ToTable("inbox_state", SharedSchemaName);
            c.AuditIgnore();
        });
        modelBuilder.AddOutboxStateEntity(c =>
        {
            c.ToTable("outbox_state", SharedSchemaName);
            c.AuditIgnore();
        });
        modelBuilder.AddOutboxMessageEntity(c =>
        {
            c.ToTable("outbox_message", SharedSchemaName);
            c.AuditIgnore();
        });

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("ModularMonolith.CategoryManagement.Infrastructure"));
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("ModularMonolith.Identity.Infrastructure"));
    }
}
