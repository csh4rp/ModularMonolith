using System.Security.Claims;
using ModularMonolith.Shared.Identity;

namespace ModularMonolith.Shared.RestApi.Middlewares;

internal sealed class IdentityMiddleware : IMiddleware
{
    private readonly IIdentityContextSetter _identityContextSetter;

    public IdentityMiddleware(IIdentityContextSetter identityContextSetter) =>
        _identityContextSetter = identityContextSetter;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity is null || !context.User.Identity.IsAuthenticated)
        {
            await next(context);
            return;
        }

        var subject = context.User.FindFirstValue("sub");

        var identityContext = new IdentityContext(subject!);
        _identityContextSetter.Set(identityContext);

        await next(context);
    }
}
