using Microsoft.EntityFrameworkCore.Storage;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

internal sealed class TransactionalScope : ITransactionalScope
{
    public static readonly AsyncLocal<TransactionalScope> Current = new();

    private IDbContextTransaction? _transaction;

    public TransactionalScope() => Current.Value = this;

    public ValueTask CompleteAsync(CancellationToken cancellationToken) => _transaction is null 
        ? new ValueTask() 
        : new ValueTask(_transaction.CommitAsync(cancellationToken));
    
    public async ValueTask DisposeAsync()
    {
        if (Current.Value is null)
        {
            return;
        }

        await Current.Value.DisposeAsync();
    }

    public void EnlistTransaction(IDbContextTransaction transaction)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("Transaction was already enlisted");
        }

        _transaction = transaction;
    }
}
