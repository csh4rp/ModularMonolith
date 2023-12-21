using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Api.Models.Errors;

public class ValidationErrorResponse : ProblemDetails
{
    public ValidationErrorResponse(string instance, string detail, string traceId, IEnumerable<PropertyError> errors)
    {
        Status = 400;
        Title = "One or more validation errors occurred";
        Detail = detail;
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        Instance = instance;
        TraceId = traceId;
        Errors = errors as IReadOnlyList<PropertyError> ?? errors.ToList();
    }

    public string TraceId { get; }

    public IReadOnlyList<PropertyError> Errors { get; }
}
