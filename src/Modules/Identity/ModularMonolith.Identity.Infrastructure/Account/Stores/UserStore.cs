﻿using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Identity.Domain.Common.Entities;
using ModularMonolith.Identity.Domain.Roles;
using ModularMonolith.Identity.Domain.Users;

namespace ModularMonolith.Identity.Infrastructure.Account.Stores;

internal sealed class UserStore : IUserLoginStore<User>,
    IUserClaimStore<User>,
    IUserPasswordStore<User>,
    IUserSecurityStampStore<User>,
    IUserEmailStore<User>,
    IUserLockoutStore<User>,
    IUserPhoneNumberStore<User>,
    IQueryableUserStore<User>,
    IUserTwoFactorStore<User>,
    IUserAuthenticationTokenStore<User>,
    IUserAuthenticatorKeyStore<User>,
    IUserTwoFactorRecoveryCodeStore<User>,
    IUserRoleStore<User>
{
    private readonly DbContext _dbContext;
    private readonly IdentityErrorDescriber _identityErrorDescriber;

    public UserStore(DbContext dbContext, IdentityErrorDescriber identityErrorDescriber)
    {
        _dbContext = dbContext;
        _identityErrorDescriber = identityErrorDescriber;
    }

    public void Dispose() { }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult((string?)user.UserName);

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(userName);

        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult((string?)user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(normalizedName);

        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Set<User>().Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Set<User>().Attach(user);
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        _dbContext.Set<User>().Update(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(_identityErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Set<User>().Remove(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(_identityErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
        _dbContext.Set<User>().FindAsync([Guid.Parse(userId)], cancellationToken).AsTask();

    public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);

    public async Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
    {
        var claims = await _dbContext.Set<UserClaim>()
            .Where(uc => uc.UserId == user.Id)
            .ToListAsync(cancellationToken);

        return claims.Select(uc => new Claim(uc.ClaimType!, uc.ClaimValue!)).ToList();
    }

    public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        var userClaims = claims.Select(c => new UserClaim(user.Id, c.Type, c.Value));

        _dbContext.Set<UserClaim>().AddRange(userClaims);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        var claimToReplace = await _dbContext.Set<UserClaim>().FirstOrDefaultAsync(uc =>
            uc.UserId == user.Id && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value, cancellationToken);

        if (claimToReplace is null)
        {
            throw new InvalidOperationException("Claim to be replaced does not exist");
        }

        claimToReplace.ClaimType = newClaim.Type;
        claimToReplace.ClaimValue = newClaim.Value;

        _dbContext.Set<UserClaim>().Update(claimToReplace);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        var userClaims = _dbContext.Set<UserClaim>().Where(u => u.UserId == user.Id);

        foreach (var claim in claims)
        {
            var claimToRemove = userClaims.FirstOrDefault(c => c.ClaimType == claim.Type);
            if (claimToRemove is null)
            {
                continue;
            }

            _dbContext.Set<UserClaim>().Remove(claimToRemove);
        }

        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken) =>
        await _dbContext.Set<UserClaim>()
            .Where(uc => uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value)
            .Select(uc => uc.User!)
            .ToListAsync(cancellationToken);

    public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        var roleId = await _dbContext.Set<Role>().Where(r => r.NormalizedName == roleName)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var userRole = new UserRole { UserId = user.Id, RoleId = roleId };
        _dbContext.Set<UserRole>().Add(userRole);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken) =>
        _dbContext.Set<UserRole>().Where(ur => ur.UserId == user.Id && ur.Role!.NormalizedName == roleName)
            .DeleteFromQueryAsync(cancellationToken);

    public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken) =>
        await _dbContext.Set<UserRole>().Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role!.NormalizedName!)
            .ToListAsync(cancellationToken);

    public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken) =>
        _dbContext.Set<UserRole>().AnyAsync(ur => ur.UserId == user.Id && ur.Role!.NormalizedName == roleName,
            cancellationToken);

    public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken) =>
        await _dbContext.Set<UserRole>().Where(ur => ur.Role!.NormalizedName == roleName)
            .Select(ur => ur.User!).ToListAsync(cancellationToken);

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.LockoutEnd);

    public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.AccessFailedCount);

    public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.LockoutEnabled);

    public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
    {
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        var userLogin = new UserLogin
        {
            UserId = user.Id,
            LoginProvider = login.LoginProvider,
            ProviderKey = login.ProviderKey,
            ProviderDisplayName = login.ProviderDisplayName
        };

        _dbContext.Set<UserLogin>().Add(userLogin);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveLoginAsync(User user,
        string loginProvider,
        string providerKey,
        CancellationToken cancellationToken)
    {
        var userLogin = _dbContext.Set<UserLogin>().FirstOrDefault(ul =>
            ul.UserId == user.Id && ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey);

        if (userLogin is null)
        {
            return Task.CompletedTask;
        }

        _dbContext.Set<UserLogin>().Remove(userLogin);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
    {
        var userLogins = await _dbContext.Set<UserLogin>()
            .Where(ul => ul.UserId == user.Id)
            .ToListAsync(cancellationToken);

        return userLogins.Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName))
            .ToList();
    }

    public async Task<User?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        var userLogin = await _dbContext.Set<UserLogin>().FirstOrDefaultAsync(ul =>
            ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey, cancellationToken);

        return userLogin is null
            ? null
            : await _dbContext.Set<User>().FindAsync([userLogin.UserId], cancellationToken);
    }

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(passwordHash);

        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult((string?)user.PasswordHash);

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult((string?)user.SecurityStamp);

    public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult((string?)user.Email);

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        _dbContext.Set<User>().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult((string?)user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(normalizedEmail);

        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task SetPhoneNumberAsync(User user, string? phoneNumber, CancellationToken cancellationToken)
    {
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    public Task<string?> GetPhoneNumberAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PhoneNumber);

    public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PhoneNumberConfirmed);

    public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
    {
        user.PhoneNumberConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public IQueryable<User> Users => _dbContext.Set<User>();

    public Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.TwoFactorEnabled);

    public Task SetTokenAsync(User user, string loginProvider, string name, string? value,
        CancellationToken cancellationToken)
    {
        var userToken = new UserToken { UserId = user.Id, LoginProvider = loginProvider, Name = name, Value = value };

        _dbContext.Set<UserToken>().Add(userToken);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken) =>
        _dbContext.Set<UserToken>().Where(ut =>
                ut.UserId == user.Id
                && ut.LoginProvider == loginProvider
                && ut.Name == name)
            .ExecuteDeleteAsync(cancellationToken);

    public Task<string?> GetTokenAsync(User user, string loginProvider, string name,
        CancellationToken cancellationToken) =>
        _dbContext.Set<UserToken>().Where(ut =>
                ut.UserId == user.Id
                && ut.LoginProvider == loginProvider
                && ut.Name == name)
            .Select(ut => ut.Value)
            .FirstOrDefaultAsync(cancellationToken);

    public Task SetAuthenticatorKeyAsync(User user, string key, CancellationToken cancellationToken) =>
        SetTokenAsync(user, "[UserStore]", "AuthenticatorKey", key, cancellationToken);

    public Task<string?> GetAuthenticatorKeyAsync(User user, CancellationToken cancellationToken) =>
        GetTokenAsync(user, "[UserStore]", "AuthenticatorKey", cancellationToken);

    public Task ReplaceCodesAsync(User user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        var str = string.Join(";", recoveryCodes);
        return SetTokenAsync(user, "[UserStore]", "RecoveryCodes", str, cancellationToken);
    }

    public async Task<bool> RedeemCodeAsync(User user, string code, CancellationToken cancellationToken)
    {
        var source = await GetTokenAsync(user, "[UserStore]", "RecoveryCodes", cancellationToken)
                     ?? string.Empty;

        var codes = source.Split(';');
        if (!codes.Contains(code))
        {
            return false;
        }

        await ReplaceCodesAsync(user, codes, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<int> CountCodesAsync(User user, CancellationToken cancellationToken)
    {
        var text = await GetTokenAsync(user, "[UserStore]", "RecoveryCodes", cancellationToken)
                   ?? string.Empty;

        return text.Length <= 0 ? 0 : text.AsSpan().Count(';') + 1;
    }
}
