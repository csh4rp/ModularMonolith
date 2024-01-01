using System.Security.Claims;
using ModularMonolith.Shared.Application.Identity;

namespace ModularMonolith.Shared.Api.Middlewares;

public class IdentityMiddleware : IMiddleware
{
    private readonly IIdentityContextSetter _identityContextSetter;

    public IdentityMiddleware(IIdentityContextSetter identityContextSetter)
    {
        _identityContextSetter = identityContextSetter;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity is null || !context.User.Identity.IsAuthenticated)
        {
            await next(context);
            return;
        }

        var userId = context.User.FindFirstValue("id");
        var userName = context.User.FindFirstValue("sub");

        var identityContext = new IdentityContext(Guid.Parse(userId!), userName!);
        _identityContextSetter.Set(identityContext);

        await next(context);
    }
}
