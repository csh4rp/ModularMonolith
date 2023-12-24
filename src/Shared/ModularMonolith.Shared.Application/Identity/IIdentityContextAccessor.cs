namespace ModularMonolith.Shared.Application.Identity;

public interface IIdentityContextAccessor
{
    public IdentityContext? Context { get; }
}
