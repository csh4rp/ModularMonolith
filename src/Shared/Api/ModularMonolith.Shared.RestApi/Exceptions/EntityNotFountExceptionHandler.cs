using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.Shared.RestApi.Exceptions;

internal sealed class EntityNotFountExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not EntityNotFoundException notFoundException)
        {
            return false;
        }

        var currentActivity = Activity.Current;
        var traceId = currentActivity?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var response = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Detail = $"{notFoundException.EntityType} was not found",
            Title = "Entity was not found",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            Status = StatusCodes.Status404NotFound,
            Extensions = new Dictionary<string, object?>
            {
                { "traceId", traceId },
                { "timestamp", DateTimeOffset.UtcNow },
                { "entityId", notFoundException.EntityId }
            }
        };

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
