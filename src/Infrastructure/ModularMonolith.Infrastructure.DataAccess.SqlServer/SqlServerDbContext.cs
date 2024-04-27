using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.AuditTrail.EntityFramework.SqlServer.EntityConfigurations;
using ModularMonolith.Shared.Events.EntityFramework.SqlServer.EntityConfigurations;

namespace ModularMonolith.Infrastructure.DataAccess.SqlServer;

public sealed class SqlServerDbContext : DbContext
{
    private const string SharedSchemaName = "Shared";

    public SqlServerDbContext(DbContextOptions<DbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventLogEntityTypeConfiguration(schemaName: SharedSchemaName))
            .ApplyConfiguration(new SqlServerAuditLogEntityTypeConfiguration(schemaName: SharedSchemaName));

        modelBuilder.AddInboxStateEntity(c =>
        {
            c.ToTable("InboxState", SharedSchemaName);
            c.AuditIgnore();
        });
        modelBuilder.AddOutboxStateEntity(c =>
        {
            c.ToTable("OutboxState", SharedSchemaName);
            c.AuditIgnore();
        });
        modelBuilder.AddOutboxMessageEntity(c =>
        {
            c.ToTable("OutboxMessage", SharedSchemaName);
            c.AuditIgnore();
        });

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("ModularMonolith.CategoryManagement.Infrastructure"));
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("ModularMonolith.Identity.Infrastructure"));
    }


}
