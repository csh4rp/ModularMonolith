namespace ModularMonolith.Shared.Application.Abstract;

public interface IUnitOfWorkFactory
{
   Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = new());
}
