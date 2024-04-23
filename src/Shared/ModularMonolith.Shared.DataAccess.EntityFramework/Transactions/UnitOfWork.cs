using Microsoft.EntityFrameworkCore.Storage;
using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Transactions;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbContextTransaction _transaction;

    public UnitOfWork(IDbContextTransaction transaction) => _transaction = transaction;

    public Task CompleteAsync(CancellationToken cancellationToken) => _transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}
