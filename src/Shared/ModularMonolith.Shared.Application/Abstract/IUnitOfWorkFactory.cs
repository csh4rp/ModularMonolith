namespace ModularMonolith.Shared.Application.Abstract;

public interface IUnitOfWorkFactory
{
    ValueTask<IUnitOfWork> CreateAsync(CancellationToken cancellationToken);
}
