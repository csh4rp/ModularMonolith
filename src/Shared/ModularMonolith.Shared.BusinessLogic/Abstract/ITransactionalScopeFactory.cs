namespace ModularMonolith.Shared.BusinessLogic.Abstract;

public interface ITransactionalScopeFactory
{
    ITransactionalScope Create();
}
