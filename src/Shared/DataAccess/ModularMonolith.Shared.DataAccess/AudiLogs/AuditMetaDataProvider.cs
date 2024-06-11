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
        Debug.Assert(Activity.Current is not null);

        var activity = Activity.Current;
        var httpContext = _httpContextAccessor.HttpContext;
        var identityContext = _identityContextAccessor.IdentityContext;

        var ipAddress = httpContext?.Connection.RemoteIpAddress;
        var extraData = GetExtraData();

        return new AuditMetaData
        {
            Subject = identityContext?.Subject,
            OperationName = Activity.Current.OperationName,
            TraceId = activity.TraceId,
            SpanId = activity.SpanId,
            ParentSpanId = activity.Parent is null
                ? null
                : activity.ParentSpanId,
            IpAddress = ipAddress,
            Uri = _httpContextAccessor.HttpContext is null 
                ? null 
                : new Uri(_httpContextAccessor.HttpContext.Request.GetEncodedUrl()),
            // ExtraData = extraData
        };
    }

    protected virtual Dictionary<string, object> GetExtraData()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var extraData = new Dictionary<string, object>();
        
        var userAgent = httpContext?.Request.Headers.UserAgent;
        if (userAgent.HasValue)
        {
            extraData["user-agent"] = userAgent;
        }

        return extraData;
    }
}
