using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IPermissionService
{
    public Task<List<Permission>> GetPermissions();
    public Task<Permission> GetPermissionByName(string name);
    public Task<Permission> AddPermission(string name);
    public Task UpdatePermission(string currentName, string newName);
    public Task DeletePermission(string name);
}