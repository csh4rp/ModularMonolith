using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Shared.Contracts.Errors;

namespace ModularMonolith.Shared.Api.Models.Errors;

public class ValidationErrorResponse : ProblemDetails
{
    public ValidationErrorResponse(string instance, PropertyError[] errors)
    {
        Status = 400;
        Title = "";
        Detail = "";
        Type = "";
        Instance = instance;
        Errors = errors;
    }
    
    public PropertyError[] Errors { get; }
}
