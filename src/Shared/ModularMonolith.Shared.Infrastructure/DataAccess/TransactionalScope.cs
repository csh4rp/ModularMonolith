using Microsoft.EntityFrameworkCore.Storage;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

internal sealed class TransactionalScope : ITransactionalScope
{
    private static readonly AsyncLocal<IDbContextTransaction> Transaction = new();
    
    public ValueTask CompleteAsync(CancellationToken cancellationToken) => Transaction.Value is null 
        ? new ValueTask() 
        : new ValueTask(Transaction.Value.CommitAsync(cancellationToken));

    public void Dispose()
    {
        Transaction.Value?.Dispose();
    }
    
    public ValueTask DisposeAsync() => Transaction.Value?.DisposeAsync() ?? new ValueTask();

    public static void EnlistTransaction(IDbContextTransaction transaction)
    {
        if (Transaction.Value is not null)
        {
            throw new InvalidOperationException("Transaction was already enlisted");
        }

        Transaction.Value = transaction;
    }
}
