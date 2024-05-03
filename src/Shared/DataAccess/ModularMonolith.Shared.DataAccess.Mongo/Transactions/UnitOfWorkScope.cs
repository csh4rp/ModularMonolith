using ModularMonolith.Shared.Application.Abstract;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Transactions;

internal sealed class UnitOfWorkScope : IUnitOfWorkScope
{
    public static readonly AsyncLocal<UnitOfWorkScope> Current = new();

    private readonly IClientSessionHandle _sessionHandle;

    public UnitOfWorkScope(IClientSessionHandle sessionHandle) => _sessionHandle = sessionHandle;

    public ValueTask DisposeAsync()
    {
        if (_sessionHandle.IsInTransaction)
        {
            return new ValueTask(_sessionHandle.AbortTransactionAsync());
        }

        _sessionHandle.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        if (_sessionHandle.IsInTransaction)
        {
            await _sessionHandle.CommitTransactionAsync(cancellationToken);
        }
    }
}
