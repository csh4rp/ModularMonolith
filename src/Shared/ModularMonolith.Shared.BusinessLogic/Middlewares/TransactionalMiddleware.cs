using MediatR;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.BusinessLogic.Middlewares;

internal sealed class TransactionalMiddleware<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ITransactionalScope _transactionalScope;

    public TransactionalMiddleware(ITransactionalScope transactionalScope) => _transactionalScope = transactionalScope;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();

        await _transactionalScope.CompleteAsync(cancellationToken);
        
        return result;
    }
}
