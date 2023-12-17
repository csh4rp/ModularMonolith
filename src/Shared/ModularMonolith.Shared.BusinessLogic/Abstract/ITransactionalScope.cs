namespace ModularMonolith.Shared.BusinessLogic.Abstract;

public interface ITransactionalScope : IAsyncDisposable
{
    Task CompleteAsync(CancellationToken cancellationToken);
}
