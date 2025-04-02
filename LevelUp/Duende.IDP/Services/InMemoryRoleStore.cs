using Microsoft.AspNetCore.Identity;

namespace Duende.IDP.Services;

public class InMemoryRoleStore : IRoleStore<IdentityRole>
{
    private readonly List<IdentityRole> _roles = new();

    public InMemoryRoleStore()
    {
        _roles.Add(new IdentityRole("Admin"));
        _roles.Add(new IdentityRole("User"));
    }

    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.Add(role);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.Remove(role);
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose() { }

    public Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken) =>
        Task.FromResult(_roles.FirstOrDefault(r => r.Id == roleId));

    public Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
        Task.FromResult(_roles.FirstOrDefault(r => r.NormalizedName == normalizedRoleName));

    public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(role.NormalizedName);

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(role.Id);

    public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(role.Name);

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);
}
