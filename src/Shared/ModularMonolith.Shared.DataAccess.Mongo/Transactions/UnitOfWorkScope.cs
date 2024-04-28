using ModularMonolith.Shared.Application.Abstract;
using MongoDB.Driver;

namespace ModularMonolith.Shared.DataAccess.Mongo.Transactions;

internal sealed class UnitOfWorkScope : IUnitOfWorkScope
{
    public static AsyncLocal<UnitOfWorkScope> Current { get; } = new();

    private readonly IClientSessionHandle _session;

    public UnitOfWorkScope(IClientSessionHandle session)
    {
        _session = session;
        Current.Value = this;
    }

    public ValueTask DisposeAsync()
    {
        _session.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task CompleteAsync(CancellationToken cancellationToken) =>
        _session.CommitTransactionAsync(cancellationToken);
}
