using MediatR;
using ModularMonolith.Shared.BusinessLogic.Abstract;

namespace ModularMonolith.Shared.BusinessLogic.Middlewares;

internal sealed class TransactionalMiddleware<TRequest, TResponse>(ITransactionalScopeFactory transactionalScopeFactory)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await using var scope =  transactionalScopeFactory.Create();
        
        var result = await next();

        await scope.CompleteAsync(cancellationToken);
        
        return result;
    }
}
