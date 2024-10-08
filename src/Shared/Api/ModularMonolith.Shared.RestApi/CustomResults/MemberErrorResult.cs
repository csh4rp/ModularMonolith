﻿using System.Diagnostics;
using ModularMonolith.Shared.Contracts.Errors;
using ModularMonolith.Shared.RestApi.Models.Errors;

namespace ModularMonolith.Shared.RestApi.CustomResults;

public sealed class MemberErrorResult : IResult
{
    private readonly MemberError[] _errors;

    public MemberErrorResult(params MemberError[] errors) => _errors = errors;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        var currentActivity = Activity.Current;
        var traceId = currentActivity?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        var response = new ValidationErrorResponse(httpContext.Request.Path, traceId, _errors);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return httpContext.Response.WriteAsJsonAsync(response);
    }
}
