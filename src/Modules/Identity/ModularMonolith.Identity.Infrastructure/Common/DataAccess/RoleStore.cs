using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Common.Entities;

namespace ModularMonolith.Identity.Infrastructure.Common.DataAccess;

internal sealed class RoleStore : IRoleStore<Role>
{
    private readonly IdentityDbContext _identityDbContext;
    private readonly IdentityErrorDescriber _identityErrorDescriber;
    
    public RoleStore(IdentityDbContext identityDbContext, IdentityErrorDescriber identityErrorDescriber)
    {
        _identityDbContext = identityDbContext;
        _identityErrorDescriber = identityErrorDescriber;
    }

    public void Dispose() => _identityDbContext.Dispose();

    public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
    {
        _identityDbContext.Roles.Add(role);
        await _identityDbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        try
        {

            _identityDbContext.Roles.Update(role);
            await _identityDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(_identityErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
    {
        _identityDbContext.Roles.Remove(role);
        await _identityDbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken) =>
        Task.FromResult(role.Id.ToString());

    public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken) => Task.FromResult((string?)role.Name);

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
        _identityDbContext.Roles.FindAsync([Guid.Parse(roleId)], cancellationToken: cancellationToken).AsTask();

    public Task<Role?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) => 
        _identityDbContext.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
}
