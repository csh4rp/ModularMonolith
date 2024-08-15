using Microsoft.EntityFrameworkCore.Storage;
using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Transactions;

public sealed class UnitOfWorkScope : IUnitOfWorkScope
{
    private readonly IDbContextTransaction _transaction;

    public UnitOfWorkScope(IDbContextTransaction transaction) => _transaction = transaction;

    public Task CompleteAsync(CancellationToken cancellationToken) => _transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}
