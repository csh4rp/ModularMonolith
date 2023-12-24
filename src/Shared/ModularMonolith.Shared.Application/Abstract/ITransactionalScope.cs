namespace ModularMonolith.Shared.Application.Abstract;

public interface ITransactionalScope : IAsyncDisposable
{
    Task CompleteAsync(CancellationToken cancellationToken);
}
