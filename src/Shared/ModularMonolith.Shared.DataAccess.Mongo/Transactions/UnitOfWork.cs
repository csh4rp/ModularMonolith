using ModularMonolith.Shared.Application.Abstract;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Transactions;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly IClientSessionHandle _session;

    public UnitOfWork(IClientSessionHandle session) => _session = session;

    public ValueTask DisposeAsync()
    {
        _session.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task CompleteAsync(CancellationToken cancellationToken) =>
        _session.CommitTransactionAsync(cancellationToken);
}
