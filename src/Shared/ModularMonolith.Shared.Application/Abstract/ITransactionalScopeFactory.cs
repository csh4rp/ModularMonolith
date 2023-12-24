namespace ModularMonolith.Shared.Application.Abstract;

public interface ITransactionalScopeFactory
{
    ITransactionalScope Create();
}
