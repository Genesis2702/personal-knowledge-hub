using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IRoleService
{
    public Task<List<Role>> GetRoles(CancellationToken cancellationToken);
    public Task<Role> GetRoleById(int id, CancellationToken cancellationToken);
    public Task<Role> AddRole(string name, CancellationToken cancellationToken);
    public Task UpdateRoleById(int id, string newName, CancellationToken cancellationToken);
    public Task DeleteRoleById(int id, CancellationToken cancellationToken);
    public Task<Role> AddPermissionToRole(int roleId, int permissionId, CancellationToken cancellationToken);
    public Task RemovePermissionFromRole(int roleId, int permissionId, CancellationToken cancellationToken);
}