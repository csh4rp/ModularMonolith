using ModularMonolith.Shared.Application.Identity;

namespace ModularMonolith.Shared.Infrastructure.Identity;

internal sealed class IdentityContextAccessor : IIdentityContextAccessor, IIdentityContextSetter
{
    public IdentityContext? Context { get; private set; }

    public void Set(IdentityContext identityContext)
    {
        if (Context is not null)
        {
            throw new InvalidOperationException("IdentityContext was already set");
        }

        Context = identityContext;
    }
}
