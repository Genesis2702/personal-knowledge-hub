using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IPermissionRepository
{
    public Task<List<Permission>> GetPermissionsAsync(CancellationToken cancellationToken);
    public Task<Permission?> GetPermissionByIdAsync(int id, CancellationToken cancellationToken);
    public Task<Permission> AddPermissionAsync(Permission permission, CancellationToken cancellationToken);
    public Task UpdatePermissionAsync(Permission permission, string name, CancellationToken cancellationToken);
    public Task DeletePermissionAsync(Permission permission, CancellationToken cancellationToken);
    public Task<bool> IsPermissionExistAsync(string name, CancellationToken cancellationToken);
}