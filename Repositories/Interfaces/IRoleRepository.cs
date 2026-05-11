using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IRoleRepository
{
    public Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken);
    public Task<Role?> GetRoleByIdAsync(int id, CancellationToken cancellationToken);
    public Task<Role> AddRoleAsync(Role role, CancellationToken cancellationToken);
    public Task UpdateRoleAsync(Role role, string name, CancellationToken cancellationToken);
    public Task DeleteRoleAsync(Role role, CancellationToken cancellationToken);
    public Task<bool> IsRoleExistAsync(string name, CancellationToken cancellationToken);
    public Task<Role> AddPermissionToRoleAsync(RolePermission rolePermission, CancellationToken cancellationToken);
    public Task RemovePermissionFromRoleAsync(RolePermission rolePermission, CancellationToken cancellationToken);
}