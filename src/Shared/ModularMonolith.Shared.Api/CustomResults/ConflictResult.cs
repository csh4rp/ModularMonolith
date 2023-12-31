using System.Diagnostics;
using ModularMonolith.Shared.Api.Models.Errors;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Api.CustomResults;

public class ConflictResult : IResult
{
    private ConflictError _conflictError;

    public ConflictResult(ConflictError conflictError) => _conflictError = conflictError;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        var currentActivity = Activity.Current;
        var traceId = currentActivity?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        var response = new ConflictErrorResponse(httpContext.Request.Path, traceId, _conflictError.Target);
        
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        return httpContext.Response.WriteAsJsonAsync(response);
    }
}
