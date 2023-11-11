using Microsoft.EntityFrameworkCore;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public abstract class BaseDbContext : DbContext
{
    public sealed override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        if (Database.CurrentTransaction is null)
        {
            var transaction = await Database.BeginTransactionAsync(cancellationToken);
            TransactionalScope.EnlistTransaction(transaction);
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (Database.CurrentTransaction is null)
        {
            var transaction = Database.BeginTransaction();
            TransactionalScope.EnlistTransaction(transaction);
        }

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
}
