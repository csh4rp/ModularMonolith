using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.DataAccess.Transactions;

namespace ModularMonolith.Infrastructure.DataAccess;

public abstract class BaseDbContext : DbContext
{
    public BaseDbContext(DbContextOptions<BaseDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            _ = await Database.BeginTransactionAsync(cancellationToken);
            TransactionalScope.Current.Value!.EnlistTransaction(this);
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (Database.CurrentTransaction is null && TransactionalScope.Current.Value is not null)
        {
            _ = Database.BeginTransaction();
            TransactionalScope.Current.Value!.EnlistTransaction(this);
        }

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
}
