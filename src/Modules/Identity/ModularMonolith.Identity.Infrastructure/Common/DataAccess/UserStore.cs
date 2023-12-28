using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.DataAccess;

internal sealed class UserStore : IUserClaimStore<User>
{
    private readonly IdentityDbContext _identityDbContext;

    public UserStore(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }

    public void Dispose()
    {
        _identityDbContext.Dispose();
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) => 
        Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.UserName);

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken) => throw new NotImplementedException();
}
