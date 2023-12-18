using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.BusinessLogic.Middlewares;

internal sealed class TracingMiddleware<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<TracingMiddleware<TRequest, TResponse>> _logger;

    public TracingMiddleware(ILogger<TracingMiddleware<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);

        var operationType = request switch
        {
            ICommand or ICommand<TResponse> => "Command",
            IQuery<TResponse> => "Query",
            _ => "Event"
        };

        _logger.OperationStarted(requestType.Name);

        using var activity = TracingSources.Default.CreateActivity(requestType.Name, ActivityKind.Internal);
        activity?.SetTag("OperationType", operationType);
        activity?.SetTag("RequestType", requestType.FullName);
        activity?.Start();
        
        var result = await next();

        _logger.OperationFinished(requestType.Name);

        return result;
    }
}
