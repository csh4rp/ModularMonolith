namespace ModularMonolith.Shared.Application.Abstract;

public interface IUnitOfWork : IAsyncDisposable
{
    Task CompleteAsync(CancellationToken cancellationToken);
}
