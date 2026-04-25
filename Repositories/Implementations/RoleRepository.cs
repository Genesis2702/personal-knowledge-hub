using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _dbContext;

    public RoleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _dbContext.Roles.AsNoTracking().ToListAsync();
    }

    public Task<Role?> GetRoleByIdAsync(int id)
    {
        return _dbContext.Roles.SingleOrDefaultAsync(role => role.Id == id);
    }

    public async Task<Role> AddRoleAsync(Role role)
    {
        await _dbContext.Roles.AddAsync(role);
        await _dbContext.SaveChangesAsync();
        return role;
    }

    public async Task UpdateRoleAsync(Role role, string name)
    {
        role.Name = name;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Entities.Role role)
    {
        _dbContext.Remove(role);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsRoleExistAsync(string name)
    {
        return await _dbContext.Roles.AnyAsync(role => role.Name == name);
    }
}