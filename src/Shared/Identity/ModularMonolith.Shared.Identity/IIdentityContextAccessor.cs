namespace ModularMonolith.Shared.Identity;

public interface IIdentityContextAccessor
{
    IdentityContext? IdentityContext { get; }
}
