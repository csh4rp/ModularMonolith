using Microsoft.AspNetCore.Identity;

namespace ModularMonolith.Identity.Domain.Users.Entities;

public sealed class UserClaim : IdentityUserClaim<Guid>;
