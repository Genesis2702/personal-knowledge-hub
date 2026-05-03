using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IRoleService
{
    public Task<List<Role>> GetRoles();
    public Task<Role> GetRoleById(int id);
    public Task<Role> AddRole(string name);
    public Task UpdateRoleById(int id, string newName);
    public Task DeleteRoleById(int id);
    public Task<Role> AddPermissionToRole(int roleId, int permissionId);
    public Task RemovePermissionFromRole(int roleId, int permissionId);
}