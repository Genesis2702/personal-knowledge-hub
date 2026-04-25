using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IRoleRepository
{
    public Task<List<Role>> GetRolesAsync();
    public Task<Role?> GetRoleByIdAsync(int id);
    public Task<Role> AddRoleAsync(Role role);
    public Task UpdateRoleAsync(Role role, string name);
    public Task DeleteRoleAsync(Role role);
}