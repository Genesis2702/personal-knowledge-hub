using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
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
        Role role = new Role
        {
            Name = name
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
        await _roleRepository.UpdateRoleAsync(role, newName);
    }

    public async Task DeleteRoleById(int id)
    {
        Role? role = await _roleRepository.GetRoleByIdAsync(id);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        await _roleRepository.DeleteRoleAsync(role);
    }
}