using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.Shared.Api.Exceptions;

internal sealed class EntityNotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not EntityNotFoundException entityNotFoundException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Entity not found",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            Instance = httpContext.Request.Path,
            Status = StatusCodes.Status404NotFound,
            Detail = entityNotFoundException.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
