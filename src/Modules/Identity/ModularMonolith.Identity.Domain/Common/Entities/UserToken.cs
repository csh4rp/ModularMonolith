using Microsoft.AspNetCore.Identity;

namespace ModularMonolith.Identity.Domain.Common.Entities;

public sealed class UserToken : IdentityUserToken<Guid>;
