using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using ModularMonolith.Shared.Identity;

namespace ModularMonolith.Shared.DataAccess.AudiLogs;

public class AuditMetaDataProvider : IAuditMetaDataProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IIdentityContextAccessor _identityContextAccessor;

    public AuditMetaDataProvider(IHttpContextAccessor httpContextAccessor,
        IIdentityContextAccessor identityContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _identityContextAccessor = identityContextAccessor;
    }

    public virtual AuditMetaData GetMetaData()
    {
        var activity = Activity.Current;
        var identityContext = _identityContextAccessor.IdentityContext;

        var extraData = GetExtraData();

        return new AuditMetaData
        {
            Subject = identityContext?.Subject,
            OperationName = activity?.OperationName,
            TraceId = activity?.TraceId,
            SpanId = activity?.SpanId,
            ParentSpanId = activity?.Parent is null
                ? null
                : activity.ParentSpanId,
            ExtraData = extraData
        };
    }

    protected virtual Dictionary<string, string?> GetExtraData()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var extraData = new Dictionary<string, string?>();

        if (httpContext is not null)
        {
            extraData["user-agent"] = httpContext.Request.Headers.UserAgent.ToString();
            extraData["ip-address"] = httpContext.Connection.RemoteIpAddress?.ToString();
            extraData["uri"] = httpContext.Request.GetEncodedUrl();
        }

        return extraData;
    }
}
