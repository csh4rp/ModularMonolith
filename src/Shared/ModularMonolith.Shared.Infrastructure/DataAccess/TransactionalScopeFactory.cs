using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

internal sealed class TransactionalScopeFactory : ITransactionalScopeFactory
{
    public ITransactionalScope Create() => new TransactionalScope();
}
