using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;

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
            var transaction = await Database.BeginTransactionAsync(cancellationToken);
            TransactionalScope.Current.Value!.EnlistTransaction(transaction);
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            var transaction = Database.BeginTransaction();
            TransactionalScope.Current.Value!.EnlistTransaction(transaction);
        }

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
}
