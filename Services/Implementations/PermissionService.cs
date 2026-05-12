using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    
    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<List<Permission>> GetPermissions(CancellationToken cancellationToken)
    {
        return await _permissionRepository.GetPermissionsAsync(cancellationToken);
    }

    public async Task<Permission> GetPermissionById(int id, CancellationToken cancellationToken)
    {
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(id, cancellationToken);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        return permission;
    }

    public async Task<Permission> AddPermission(string name, CancellationToken cancellationToken)
    {
        if (await _permissionRepository.IsPermissionExistAsync(name, cancellationToken))
        {
            throw new ConflictException("Permission already existed");
        }
        Permission permission = new Permission
        {
            Name = name
        };
        return await _permissionRepository.AddPermissionAsync(permission, cancellationToken);
    }

    public async Task UpdatePermissionById(int id, string newName, CancellationToken cancellationToken)
    {
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(id, cancellationToken);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        await _permissionRepository.UpdatePermissionAsync(permission, newName, cancellationToken);
    }

    public async Task DeletePermissionById(int id, CancellationToken cancellationToken)
    {
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(id, cancellationToken);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        await _permissionRepository.DeletePermissionAsync(permission, cancellationToken);
    }
}