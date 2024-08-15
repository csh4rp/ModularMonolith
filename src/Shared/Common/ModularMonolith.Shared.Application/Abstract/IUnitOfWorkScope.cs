namespace ModularMonolith.Shared.Application.Abstract;

public interface IUnitOfWorkScope : IAsyncDisposable
{
    Task CompleteAsync(CancellationToken cancellationToken);
}
