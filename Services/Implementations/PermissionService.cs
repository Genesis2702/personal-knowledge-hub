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

    public async Task<Permission> GetPermissionByName(string name)
    {
        Permission? permission = await _permissionRepository.GetPermissionByNameAsync(name);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        return permission;
    }

    public async Task<Permission> AddPermission(string name)
    {
        Permission permission = new Permission
        {
            Name = name
        };
        return await _permissionRepository.AddPermissionAsync(permission);
    }

    public async Task UpdatePermission(string currentName, string newName)
    {
        Permission? permission = await _permissionRepository.GetPermissionByNameAsync(currentName);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        await _permissionRepository.UpdatePermissionAsync(permission, newName);
    }

    public async Task DeletePermission(string name)
    {
        Permission? permission = await _permissionRepository.GetPermissionByNameAsync(name);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        await _permissionRepository.DeletePermissionAsync(permission);
    }
}