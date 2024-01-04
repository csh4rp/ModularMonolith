using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public sealed class SharedDbContext : BaseDbContext, IEventLogDbContext
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
    {
    }

    public DbSet<EventLog> EventLogs { get; set; } = default!;

    public DbSet<EventLogLock> EventLogLocks { get; set; } = default!;

    public DbSet<EventCorrelationLock> EventCorrelationLocks { get; set; } = default!;

    public DbSet<EventLogPublishAttempt> EventLogPublishAttempts { get; set; } = default!;

    public DbSet<AuditLog> AuditLogs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventCorrelationLockEntityTypeConfiguration(false, schema: "shared"))
            .ApplyConfiguration(new EventLogLockEntityTypeConfiguration(false, schema: "shared"))
            .ApplyConfiguration(new EventLogEntityTypeConfiguration(false, schema: "shared"))
            .ApplyConfiguration(new EventLogPublishAttemptEntityTypeConfiguration(false, schema: "shared"))
            .ApplyConfiguration(new AuditLogEntityTypeConfiguration(false, schema: "shared"));

        modelBuilder.HasDefaultSchema("shared");
    }
}
