using ModularMonolith.Identity.Domain.Roles;

namespace ModularMonolith.Identity.Domain.Users;

public sealed class UserRole
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public Guid RoleId { get; set; }

    public Role? Role { get; set; }
}
