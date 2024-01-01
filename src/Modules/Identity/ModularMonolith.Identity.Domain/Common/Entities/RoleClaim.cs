namespace ModularMonolith.Identity.Domain.Common.Entities;

public sealed class RoleClaim
{
    private RoleClaim()
    {
        ClaimType = default!;
        ClaimValue = default!;
    }

    public RoleClaim(Guid roleId, string claimType, string claimValue)
    {
        RoleId = roleId;
        ClaimType = claimType;
        ClaimValue = claimValue;
    }

    public Guid Id { get; set; }

    public Guid RoleId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }
}
