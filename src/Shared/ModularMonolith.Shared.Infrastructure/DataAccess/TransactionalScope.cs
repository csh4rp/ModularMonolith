using Microsoft.EntityFrameworkCore.Storage;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

internal sealed class TransactionalScope : ITransactionalScope
{
    private static readonly AsyncLocal<IDbContextTransaction> CurrentTransaction = new();
    
    public ValueTask CompleteAsync(CancellationToken cancellationToken) => CurrentTransaction.Value is null 
        ? new ValueTask() 
        : new ValueTask(CurrentTransaction.Value.CommitAsync(cancellationToken));

    public void Dispose() => CurrentTransaction.Value?.Dispose();

    public async ValueTask DisposeAsync()
    {
        if (CurrentTransaction.Value is null)
        {
            return;
        }

        await CurrentTransaction.Value.DisposeAsync();
    }

    public static void EnlistTransaction(IDbContextTransaction transaction)
    {
        if (CurrentTransaction.Value is not null)
        {
            throw new InvalidOperationException("Transaction was already enlisted");
        }

        CurrentTransaction.Value = transaction;
    }
}
