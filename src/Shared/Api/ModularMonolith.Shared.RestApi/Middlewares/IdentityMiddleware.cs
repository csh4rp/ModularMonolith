using System.Security.Claims;
using ModularMonolith.Shared.Identity;

namespace ModularMonolith.Shared.RestApi.Middlewares;

internal sealed class IdentityMiddleware : IMiddleware
{
    private readonly IIdentityContextSetter _identityContextSetter;
    private readonly IConfiguration _configuration;

    public IdentityMiddleware(IIdentityContextSetter identityContextSetter, IConfiguration configuration)
    {
        _identityContextSetter = identityContextSetter;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity is null || !context.User.Identity.IsAuthenticated)
        {
            await next(context);
            return;
        }

        var permissionClaimName = _configuration.GetSection("Identity:PermissionClaimName")
            .Get<string>() ?? ClaimTypes.Role;

        var subject = context.User.FindFirstValue("sub");
        var permissions = context.User.FindAll(permissionClaimName)
            .Select(r => Permission.Parse(r.Value))
            .ToList();

        var identityContext = new IdentityContext(subject!, permissions);
        _identityContextSetter.Set(identityContext);

        await next(context);
    }
}
