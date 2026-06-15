using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILogger<PermissionService> _logger;
    
    public PermissionService(IPermissionRepository permissionRepository, ILogger<PermissionService> logger)
    {
        _permissionRepository = permissionRepository;
        _logger = logger;
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
        _logger.LogInformation("Permission {name} added successfully", permission.Name);
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
        _logger.LogInformation("Permission {name} updated successfully", newName);
    }

    public async Task DeletePermissionById(int id, CancellationToken cancellationToken)
    {
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(id, cancellationToken);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        await _permissionRepository.DeletePermissionAsync(permission, cancellationToken);
        _logger.LogInformation("Permission {name} deleted successfully", permission.Name);
    }
}