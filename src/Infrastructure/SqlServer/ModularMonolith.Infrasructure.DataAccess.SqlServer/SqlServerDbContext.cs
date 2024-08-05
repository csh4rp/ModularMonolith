using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.EntityConfigurations;

namespace ModularMonolith.Infrastructure.DataAccess.SqlServer;

public sealed class SqlServerDbContext : DbContext
{
    private const string SharedSchemaName = "Shared";

    private readonly ConfigurationAssemblyCollection _configurationAssemblyCollection;

    public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options,
        ConfigurationAssemblyCollection configurationAssemblyCollection)
        : base(options) =>
        _configurationAssemblyCollection = configurationAssemblyCollection;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventLogEntityTypeConfiguration(schemaName: SharedSchemaName))
        .ApplyConfiguration(new AuditLogEntityTypeConfiguration(schemaName: SharedSchemaName));

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

        foreach (var configurationAssembly in _configurationAssemblyCollection)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(configurationAssembly);
        }
    }
}
