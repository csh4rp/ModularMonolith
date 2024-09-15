using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.EntityFramework;
using ModularMonolith.Shared.DataAccess.EntityFramework.AuditLogs;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.DataAccess.EntityFramework.Postgres.EventLogs.EntityConfigurations;

namespace ModularMonolith.Infrastructure.DataAccess.Postgres;

public sealed class PostgresDbContext : DbContext
{
    private const string SharedSchemaName = "shared";

    private readonly ConfigurationAssemblyCollection _configurationAssemblies;

    public PostgresDbContext(DbContextOptions<PostgresDbContext> options,
        ConfigurationAssemblyCollection configurationAssemblies)
        : base(options) =>
        _configurationAssemblies = configurationAssemblies;

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

        new JobSagaMap(false).Configure(modelBuilder);
        new JobTypeSagaMap(false).Configure(modelBuilder);
        new JobAttemptSagaMap(false).Configure(modelBuilder);

        modelBuilder.Entity<JobSaga>(c =>
        {
            c.AuditIgnore();
        });

        modelBuilder.Entity<JobTypeSaga>(c =>
        {
            c.AuditIgnore();
        });

        modelBuilder.Entity<JobAttemptSaga>(c =>
        {
            c.AuditIgnore();
        });

        foreach (var configurationAssembly in _configurationAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(configurationAssembly);
        }
    }
}
