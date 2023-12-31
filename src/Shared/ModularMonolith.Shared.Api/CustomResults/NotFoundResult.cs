using System.Diagnostics;
using ModularMonolith.Shared.Api.Models.Errors;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Api.CustomResults;

public class NotFoundResult : IResult
{
    private readonly EntityNotFoundError _entityNotFoundError;

    public NotFoundResult(EntityNotFoundError entityNotFoundError)
    {
        _entityNotFoundError = entityNotFoundError;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        var currentActivity = Activity.Current;
        var traceId = currentActivity?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        var response = new NotFoundErrorResponse(httpContext.Request.Path, 
            traceId, 
            _entityNotFoundError.EntityName, 
            _entityNotFoundError.EntityId.ToString()!);
        
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        return httpContext.Response.WriteAsJsonAsync(response);
    }
}
