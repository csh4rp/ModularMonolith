using Microsoft.AspNetCore.Mvc;

namespace ModularMonolith.Shared.Api.Models.Errors;

public class NotFoundErrorResponse : ProblemDetails
{
    public NotFoundErrorResponse(string instance, string traceId, string entityType, string entityId)
    {
        Status = 400;
        Title = "One or more validation errors occurred";
        Detail = "One or more validation errors occurred";
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4";
        Instance = instance;
        TraceId = traceId;
        Timestamp = DateTimeOffset.UtcNow;
        EntityType = entityType;
        EntityId = entityId;
    }

    public string TraceId { get; }

    public DateTimeOffset Timestamp { get; }
    
    public string EntityType { get; }
    
    public string EntityId { get; }
}
