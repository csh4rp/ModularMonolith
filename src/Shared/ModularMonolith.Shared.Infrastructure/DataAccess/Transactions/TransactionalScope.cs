using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;

internal sealed class TransactionalScope : ITransactionalScope
{
    public static readonly AsyncLocal<TransactionalScope?> Current = new();

    public DbContext? DbContext { get; private set; }

    public TransactionalScope() => Current.Value = this;

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        if (DbContext?.Database.CurrentTransaction is null)
        {
            return;
        }

        await DbContext.Database.CurrentTransaction.CommitAsync(cancellationToken);
        DbContext = null;
    }

    public ValueTask DisposeAsync() => Current.Value?.DbContext?.Database.CurrentTransaction is null
        ? ValueTask.CompletedTask
        : Current.Value.DbContext.Database.CurrentTransaction.DisposeAsync();

    public void EnlistTransaction(DbContext dbContext)
    {
        if (DbContext is not null)
        {
            throw new InvalidOperationException("Transaction was already enlisted");
        }

        DbContext = dbContext;
    }
}
