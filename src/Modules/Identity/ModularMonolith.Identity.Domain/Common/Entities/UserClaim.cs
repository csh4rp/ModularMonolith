namespace ModularMonolith.Identity.Domain.Common.Entities;

public sealed class UserClaim
{
    public Guid Id { get; set; }
    
    public required Guid UserId { get; set; }
    
    public User? User { get; set; }
    
    public required string ClaimType { get; set; }
    
    public required string ClaimValue { get; set; }
}
