using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IPermissionService
{
    public Task<List<Permission>> GetPermissions(CancellationToken cancellationToken);
    public Task<Permission> GetPermissionById(int id, CancellationToken cancellationToken);
    public Task<Permission> AddPermission(string name, CancellationToken cancellationToken);
    public Task UpdatePermissionById(int id, string newName, CancellationToken cancellationToken);
    public Task DeletePermissionById(int id, CancellationToken cancellationToken);
}