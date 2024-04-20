using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.AuditTrail.Storage;
using ModularMonolith.Shared.AuditTrail.EntityFramework.Postgres.EntityConfigurations;
using ModularMonolith.Shared.Events.EntityFramework.Postgres.EntityConfigurations;

namespace ModularMonolith.Infrastructure.DataAccess.Postgres;

public sealed class PostgresDbContext : BaseDbContext
{
    private const string SharedSchemaName = "shared";

    public PostgresDbContext(DbContextOptions<BaseDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventLogEntityTypeConfiguration(schemaName: SharedSchemaName))
            .ApplyConfiguration(new PostgresAuditLogEntityTypeConfiguration(schemaName: SharedSchemaName));

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
