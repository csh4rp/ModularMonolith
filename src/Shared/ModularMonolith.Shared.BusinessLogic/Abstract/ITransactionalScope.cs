namespace ModularMonolith.Shared.BusinessLogic.Abstract;

public interface ITransactionalScope : IAsyncDisposable
{
    ValueTask CompleteAsync(CancellationToken cancellationToken);
}
