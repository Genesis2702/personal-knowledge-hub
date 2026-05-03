using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IPermissionService
{
    public Task<List<Permission>> GetPermissions();
    public Task<Permission> GetPermissionById(int id);
    public Task<Permission> AddPermission(string name);
    public Task UpdatePermissionById(int id, string newName);
    public Task DeletePermissionById(int id);
}