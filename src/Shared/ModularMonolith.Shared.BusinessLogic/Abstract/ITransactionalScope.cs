namespace ModularMonolith.Shared.BusinessLogic.Abstract;

public interface ITransactionalScope : IAsyncDisposable, IDisposable
{
    ValueTask CompleteAsync(CancellationToken cancellationToken);
}
