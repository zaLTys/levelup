using Duende.IdentityServer.Test;
using Duende.IDP.Models;
using Microsoft.AspNetCore.Identity;

namespace Duende.IDP.Services;

public class InMemoryUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserTwoFactorStore<ApplicationUser>, IUserAuthenticatorKeyStore<ApplicationUser>
{
    private readonly List<ApplicationUser> _users = new();

    public InMemoryUserStore(List<TestUser> testUsers)
    {
        foreach (var testUser in testUsers)
        {
            var appUser = new ApplicationUser
            {
                Id = testUser.SubjectId,
                UserName = testUser.Username,
                NormalizedUserName = testUser.Username.ToUpper(),
                PasswordHash = testUser.Password // In a real app, hash this
            };
            _users.Add(appUser);
        }
    }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _users.Add(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _users.Remove(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose() { }

    public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));

    public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName));

    public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.NormalizedUserName);

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id);

    public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.UserName);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    // Password store methods
    public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.TwoFactorEnabled);

    public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<string> GetAuthenticatorKeyAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        // Return the stored authenticator key (if any)
        return Task.FromResult(user.AuthenticatorKey);
    }

    public Task SetAuthenticatorKeyAsync(ApplicationUser user, string key, CancellationToken cancellationToken)
    {
        // Set the authenticator key for the user
        user.AuthenticatorKey = key;
        return Task.CompletedTask;
    }

    public Task RemoveAuthenticatorKeyAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        // Remove the authenticator key for the user
        user.AuthenticatorKey = null;
        return Task.CompletedTask;
    }
}
