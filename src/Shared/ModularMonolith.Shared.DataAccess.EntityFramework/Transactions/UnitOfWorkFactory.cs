using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.DataAccess.EntityFramework.Transactions;

internal sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly DbContext _dbContext;

    public UnitOfWorkFactory(DbContext dbContext) => _dbContext = dbContext;

    public async ValueTask<IUnitOfWork> CreateAsync(CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new UnitOfWork(transaction);
    }
}
