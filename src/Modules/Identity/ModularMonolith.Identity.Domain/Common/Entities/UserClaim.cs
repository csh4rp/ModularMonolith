namespace ModularMonolith.Identity.Domain.Common.Entities;

public sealed class UserClaim
{
    private UserClaim()
    {
        ClaimType = default!;
        ClaimValue = default!;
    }
    
    public UserClaim(Guid userId, string claimType, string claimValue)
    {
        UserId = userId;
        ClaimType = claimType;
        ClaimValue = claimValue;
    }
    
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public User? User { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }
}
