using System.Reflection;
using MediatR;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Application.Middlewares;

internal sealed class TransactionalMiddleware<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionalMiddleware(IUnitOfWork unitOfWork) =>
        _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IQuery<TResponse>)
        {
            return await next();
        }

        var hasReturnType = typeof(TResponse) != typeof(Unit);

        var type = hasReturnType
            ? typeof(IRequestHandler<,>).MakeGenericType(typeof(TRequest), typeof(TResponse))
            : typeof(IRequestHandler<>).MakeGenericType(typeof(TRequest));

        if (TransactionalTypesDictionary.ShouldUseTransaction(type))
        {
            return await next();
        }

        var handlerType = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName!.StartsWith("ModularMonolith"))
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.IsAssignableTo(type));

        var hasTransactionalAttribute = handlerType?.GetCustomAttribute<TransactionalAttribute>();

        var shouldUseTransaction = hasTransactionalAttribute is not null;
        TransactionalTypesDictionary.Add(type, shouldUseTransaction);

        await using var scope = await _unitOfWork.BeginScopeAsync(cancellationToken);

        var result = await next();

        await scope.CompleteAsync(cancellationToken);

        return result;
    }
}
