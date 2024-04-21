using System.Diagnostics;
using ModularMonolith.Shared.Identity;

namespace ModularMonolith.Shared.AuditTrail;

public class AuditMetaDataProvider : IAuditMetaDataProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IIdentityContextAccessor _identityContextAccessor;

    public AuditMetaDataProvider(IHttpContextAccessor httpContextAccessor, IIdentityContextAccessor identityContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _identityContextAccessor = identityContextAccessor;
    }

    public AuditMetaData MetaData
    {
        get
        {
            Debug.Assert(Activity.Current is not null);

            var activity = Activity.Current;
            var httpContext = _httpContextAccessor.HttpContext;
            var identityContext = _identityContextAccessor.IdentityContext;

            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers.UserAgent;

            return new AuditMetaData
            {
                Subject = identityContext?.Subject,
                OperationName = Activity.Current.OperationName,
                TraceId = activity.TraceId.ToString(),
                SpanId = activity.SpanId.ToString(),
                ParentSpanId = activity.Parent is null ? null : activity.ParentSpanId.ToString(),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ExtraData = []
            };
        }
    }
}
