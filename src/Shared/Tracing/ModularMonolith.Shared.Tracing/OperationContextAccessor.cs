using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using ModularMonolith.Shared.Identity;

namespace ModularMonolith.Shared.Tracing;

internal sealed class OperationContextAccessor : IOperationContextAccessor
{
    private const string DefaultSubject = "SYSTEM";

    private readonly IIdentityContextAccessor _identityContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OperationContextAccessor(IIdentityContextAccessor identityContextAccessor,
        IHttpContextAccessor httpContextAccessor)
    {
        _identityContextAccessor = identityContextAccessor;
        _httpContextAccessor = httpContextAccessor;
    }

    public IOperationContext? OperationContext
    {
        get
        {
            var currentActivity = Activity.Current;
            if (currentActivity is null)
            {
                return null;
            }

            var httpContext = _httpContextAccessor.HttpContext;

            return new OperationContext
            {
                Subject = _identityContextAccessor.IdentityContext?.Subject ?? DefaultSubject,
                Uri = httpContext is null ? null : new Uri(httpContext.Request.GetEncodedUrl()),
                IpAddress = httpContext?.Connection.RemoteIpAddress,
                OperationName = currentActivity.OperationName,
                TraceId = currentActivity.TraceId,
                SpanId = currentActivity.SpanId,
                ParentSpanId = currentActivity.ParentSpanId,
            };
        }
    }
}
