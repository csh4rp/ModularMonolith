using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Roles;

namespace ModularMonolith.Identity.Infrastructure.Account.Stores;

internal sealed class RoleStore : IRoleStore<Role>
{
    private readonly DbContext _dbContext;
    private readonly IdentityErrorDescriber _identityErrorDescriber;

    public RoleStore(DbContext dbContext, IdentityErrorDescriber identityErrorDescriber)
    {
        _dbContext = dbContext;
        _identityErrorDescriber = identityErrorDescriber;
    }

    public void Dispose() {}

    public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
    {
        _dbContext.Set<Role>().Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Set<Role>().Update(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(_identityErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
    {
        _dbContext.Set<Role>().Remove(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult(role.Id.ToString());

    public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult((string?)role.Name);

    public Task SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(roleName);

        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult((string?)role.NormalizedName);

    public Task SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(normalizedName);

        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task<Role?> FindByIdAsync(string roleId, CancellationToken cancellationToken) =>
        _dbContext.Set<Role>().FindAsync([Guid.Parse(roleId)], cancellationToken).AsTask();

    public Task<Role?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
        _dbContext.Set<Role>().FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
}
