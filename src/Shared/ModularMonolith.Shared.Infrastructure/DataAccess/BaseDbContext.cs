using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.AuditLogs;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;
using ModularMonolith.Shared.Infrastructure.Events.DataAccess.EntityConfigurations;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public sealed override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            _ = await Database.BeginTransactionAsync(cancellationToken);
            TransactionalScope.Current.Value!.EnlistTransaction(this);
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            _ = Database.BeginTransaction();
            TransactionalScope.Current.Value!.EnlistTransaction(this);
        }

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventCorrelationLockEntityTypeConfiguration(false))
            .ApplyConfiguration(new EventLogLockEntityTypeConfiguration(false))
            .ApplyConfiguration(new EventLogEntityTypeConfiguration(false))
            .ApplyConfiguration(new AuditLogEntityTypeConfiguration(false));
    }
}
