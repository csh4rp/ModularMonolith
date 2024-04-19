using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.DataAccess.SqlServer.Transactions;

internal sealed class TransactionalScopeFactory : ITransactionalScopeFactory
{
    public ITransactionalScope Create() => new TransactionalScope();
}
