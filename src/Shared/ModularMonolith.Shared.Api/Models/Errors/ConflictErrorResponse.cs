using Microsoft.AspNetCore.Mvc;

namespace ModularMonolith.Shared.Api.Models.Errors;

public class ConflictErrorResponse : ProblemDetails
{
    public ConflictErrorResponse(string instance, string traceId, string target)
    {
        Status = 409;
        Title = "One or more validation errors occurred";
        Detail = "One or more validation errors occurred";
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";
        Instance = instance;
        TraceId = traceId;
        Timestamp = DateTimeOffset.UtcNow;
        Target = target;
    }

    public string TraceId { get; }

    public DateTimeOffset Timestamp { get; }

    public string Target { get; }
}
