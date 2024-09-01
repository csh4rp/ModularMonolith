namespace ModularMonolith.Shared.Identity;

public sealed record IdentityContext(string Subject, IReadOnlyList<Permission> Permissions)
{
    public bool HasAccessTo(string permission) => Permissions.Any(p => p.GrantsAccessTo(permission));
}
