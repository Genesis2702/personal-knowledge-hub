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

    public async Task<List<Permission>> GetPermissions()
    {
        return await _permissionRepository.GetPermissionsAsync();
    }

    public async Task<Permission> GetPermissionById(int id)
    {
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(id);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        return permission;
    }

    public async Task<Permission> AddPermission(string name)
    {
        if (await _permissionRepository.IsPermissionExistAsync(name))
        {
            throw new ConflictException("Permission already existed");
        }
        Permission permission = new Permission
        {
            Name = name
        };
        return await _permissionRepository.AddPermissionAsync(permission);
    }

    public async Task UpdatePermissionById(int id, string newName)
    {
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(id);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        await _permissionRepository.UpdatePermissionAsync(permission, newName);
    }

    public async Task DeletePermissionById(int id)
    {
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(id);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        await _permissionRepository.DeletePermissionAsync(permission);
    }
}