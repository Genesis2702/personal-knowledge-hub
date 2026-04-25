using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IPermissionRepository
{
    public Task<List<Permission>> GetPermissionsAsync();
    public Task<Permission?> GetPermissionByIdAsync(int id);
    public Task<Permission> AddPermissionAsync(Permission permission);
    public Task UpdatePermissionAsync(Permission permission, string name);
    public Task DeletePermissionAsync(Permission permission);
    public Task<bool> IsPermissionExistAsync(string name);
}