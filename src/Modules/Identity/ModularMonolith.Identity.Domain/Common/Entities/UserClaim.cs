namespace ModularMonolith.Identity.Domain.Common.Entities;

public sealed class UserClaim
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string? ClaimType { get; set; }
    
    public string? ClaimValue { get; set; }
}
