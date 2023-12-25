using Microsoft.AspNetCore.Identity;

namespace ModularMonolith.Identity.Domain.Users.Entities;

public sealed class UserToken : IdentityUserToken<Guid>;
