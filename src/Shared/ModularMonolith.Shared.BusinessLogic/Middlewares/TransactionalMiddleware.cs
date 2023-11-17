using MediatR;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.BusinessLogic.Middlewares;

internal sealed class TransactionalMiddleware<TRequest, TResponse>(ITransactionalScope transactionalScope)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();

        await transactionalScope.CompleteAsync(cancellationToken);
        
        return result;
    }
}
