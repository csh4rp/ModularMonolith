using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using ModularMonolith.Shared.Api.Models.Errors;
using ModularMonolith.Shared.Application.Exceptions;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Api.Exceptions;

internal sealed class ConflictExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ConflictException conflictException)
        {
            return false;
        }

        var currentActivity = Activity.Current;
        var traceId = currentActivity?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var response = new ValidationErrorResponse(httpContext.Request.Path, traceId, new[]
        {
            MemberError.Conflict(conflictException.Reference), 
        });

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
