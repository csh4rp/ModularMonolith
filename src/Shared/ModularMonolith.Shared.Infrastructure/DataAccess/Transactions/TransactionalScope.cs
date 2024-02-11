using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.Infrastructure.DataAccess.Transactions;

public sealed class TransactionalScope : ITransactionalScope
{
    public static readonly AsyncLocal<TransactionalScope?> Current = new();

    private DbContext? _dbContext;

    public TransactionalScope() => Current.Value = this;

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        if (_dbContext?.Database.CurrentTransaction is null)
        {
            return;
        }

        await _dbContext.Database.CurrentTransaction.CommitAsync(cancellationToken);
        _dbContext = null;
    }

    public ValueTask DisposeAsync() => Current.Value?._dbContext?.Database.CurrentTransaction is null
        ? ValueTask.CompletedTask
        : Current.Value._dbContext.Database.CurrentTransaction.DisposeAsync();

    public void EnlistTransaction(DbContext dbContext)
    {
        if (_dbContext is not null)
        {
            throw new InvalidOperationException("Transaction was already enlisted");
        }

        _dbContext = dbContext;
    }
}
