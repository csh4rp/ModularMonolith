using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.EntityConfigurations;

namespace ModularMonolith.Infrastructure.DataAccess.SqlServer;

public sealed class SqlServerDbContext : DbContext
{
    private const string SharedSchemaName = "Shared";
    
    private readonly IReadOnlyCollection<Assembly> _configurationAssemblies;

    public SqlServerDbContext(DbContextOptions<DbContext> options, IReadOnlyCollection<Assembly> configurationAssemblies) : base(options) => 
        _configurationAssemblies = configurationAssemblies;

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

        foreach (var configurationAssembly in _configurationAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(configurationAssembly);
        }
    }
}
