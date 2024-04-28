using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Transactions;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;

    public UnitOfWork(DbContext dbContext) => _dbContext = dbContext;

    public async Task<IUnitOfWorkScope> BeginScopeAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException("Transaction is already in place, cannot start another transaction");
        }

        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new UnitOfWorkScope(transaction);
    }
}
