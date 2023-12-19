using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Domain.Entities;
using ModularMonolith.Shared.Infrastructure.AuditLogs.EntityConfigurations;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.Abstract;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

namespace ModularMonolith.Shared.Infrastructure.DataAccess.Internal;

internal sealed class InternalDbContext : BaseDbContext, IEventLogDbContext
{
    public InternalDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<EventLog> EventLogs { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventCorrelationLockEntityTypeConfiguration(false))
            .ApplyConfiguration(new EventLogLockEntityTypeConfiguration(false))
            .ApplyConfiguration(new EventLogEntityTypeConfiguration(false))
            .ApplyConfiguration(new AuditLogEntityTypeConfiguration(false));
    }
}
