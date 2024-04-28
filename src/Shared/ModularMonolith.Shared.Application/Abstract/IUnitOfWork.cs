namespace ModularMonolith.Shared.Application.Abstract;

public interface IUnitOfWork
{
   Task<IUnitOfWorkScope> BeginScopeAsync(CancellationToken cancellationToken = new());
}
