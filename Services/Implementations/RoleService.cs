using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<List<Role>> GetRoles()
    {
        return await _roleRepository.GetRolesAsync();
    }

    public async Task<Role> GetRoleById(int id)
    {
        Role? role = await _roleRepository.GetRoleByIdAsync(id);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }

        return role;
    }

    public async Task<Role> AddRole(string name)
    {
        if (await _roleRepository.IsRoleExistAsync(name))
        {
            throw new ConflictException("Role already existed");
        }
        Role role = new Role
        {
            Name = name.Trim().ToUpper()
        };
        return await _roleRepository.AddRoleAsync(role);
    }

    public async Task UpdateRoleById(int id, string newName)
    {
        Role? role = await _roleRepository.GetRoleByIdAsync(id);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        if (role.Name == newName)
        {
            throw new ConflictException("Role name already existed");
        }
        await _roleRepository.UpdateRoleAsync(role, newName);
    }

    public async Task DeleteRoleById(int id)
    {
        Role? role = await _roleRepository.GetRoleByIdAsync(id);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        if (role.Name == "admin")
        {
            throw new ConflictException("Admin role cannot be deleted");
        }
        await _roleRepository.DeleteRoleAsync(role);
    }

    public async Task<Role> AddPermissionToRole(int roleId, int permissionId)
    {
        Role? role = await _roleRepository.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(permissionId);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        RolePermission rolePermission = new RolePermission
        {
            Role = role,
            Permission = permission,
            RoleId = roleId,
            PermissionId = permissionId
        };
        return await _roleRepository.AddPermissionToRoleAsync(rolePermission);
    }

    public async Task RemovePermissionFromRole(int roleId, int permissionId)
    {
        Role? role = await _roleRepository.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        Permission? permission = await _permissionRepository.GetPermissionByIdAsync(permissionId);
        if (permission == null)
        {
            throw new NotFoundException("Permission not found");
        }
        RolePermission rolePermission = new RolePermission
        {
            Role = role,
            Permission = permission,
            RoleId = roleId,
            PermissionId = permissionId
        };
        await _roleRepository.RemovePermissionFromRoleAsync(rolePermission);
    }
}