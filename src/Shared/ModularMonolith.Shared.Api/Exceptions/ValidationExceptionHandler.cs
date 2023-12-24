using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using ModularMonolith.Shared.Api.Models.Errors;
using ModularMonolith.Shared.Application.Exceptions;

namespace ModularMonolith.Shared.Api.Exceptions;

internal sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var currentActivity = Activity.Current;
        var detail = currentActivity is null
            ? "One or more validation errors occurred"
            : $"One or more validation errors occurred while handling: {currentActivity.OperationName}";
        var traceId = currentActivity?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        var errors = validationException.Errors;

        var response = new ValidationErrorResponse(httpContext.Request.Path, detail, traceId, errors);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
