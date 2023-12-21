using Microsoft.AspNetCore.Diagnostics;

namespace ModularMonolith.Shared.Api.Exceptions;

internal sealed class CatchAllExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CatchAllExceptionHandler> _logger;

    public CatchAllExceptionHandler(ILogger<CatchAllExceptionHandler> logger) => _logger = logger;

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        return ValueTask.FromResult(true);
    }
}
