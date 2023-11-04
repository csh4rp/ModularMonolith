namespace ModularMonolith.Shared.BusinessLogic.Identity;

public interface IIdentityContextAccessor
{
    public IdentityContext? Context { get; }
}
