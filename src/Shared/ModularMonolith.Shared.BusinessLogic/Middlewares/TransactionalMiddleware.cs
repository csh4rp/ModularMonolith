using MediatR;
using ModularMonolith.Shared.BusinessLogic.Abstract;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.BusinessLogic.Middlewares;

internal sealed class TransactionalMiddleware<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ITransactionalScopeFactory _transactionalScopeFactory;

    public TransactionalMiddleware(ITransactionalScopeFactory transactionalScopeFactory) =>
        _transactionalScopeFactory = transactionalScopeFactory;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IQuery<TResponse>)
        {
            return await next();
        }

        await using var scope = _transactionalScopeFactory.Create();

        var result = await next();

        await scope.CompleteAsync(cancellationToken);

        return result;
    }
}
