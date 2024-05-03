﻿using System.Collections.Concurrent;
using System.Reflection;
using MediatR;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Application.Middlewares;

internal sealed class TransactionalMiddleware<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, bool> TransactionBehaviourLookup = new();

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

        var type = typeof(IRequestHandler<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));

        if (!TransactionBehaviourLookup.TryGetValue(type, out var shouldUseTransaction))
        {
            var handlerType = Assembly.GetEntryAssembly()!
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(a => a.GetTypes())
                .First(t => t.IsAssignableTo(type));

            var hasTransactionalAttribute = handlerType.GetCustomAttribute<TransactionalAttribute>();

            shouldUseTransaction = hasTransactionalAttribute is not null;
            TransactionBehaviourLookup.TryAdd(type, shouldUseTransaction);
        }

        if (!shouldUseTransaction)
        {
            return await next();
        }

        await using var scope = await _unitOfWork.BeginScopeAsync(cancellationToken);

        var result = await next();

        await scope.CompleteAsync(cancellationToken);

        return result;
    }
}