using ModularMonolith.Shared.Application.Abstract;

namespace ModularMonolith.Shared.DataAccess.Transactions;

internal sealed class TransactionalScopeFactory : ITransactionalScopeFactory
{
    public ITransactionalScope Create() => new TransactionalScope();
}
