namespace ModularMonolith.Shared.Identity;

internal sealed class IdentityContextWrapper : IIdentityContextAccessor, IIdentityContextSetter
{
    public IdentityContext? IdentityContext { get; private set; }

    public void Set(IdentityContext identityContext)
    {
        if (IdentityContext is not null)
        {
            throw new InvalidOperationException("Identity context is already set");
        }

        IdentityContext = identityContext;
    }
}
