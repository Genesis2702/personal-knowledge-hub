using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations;

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _dbContext;

    public PermissionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;   
    }

    public async Task<List<Permission>> GetPermissionsAsync()
    {
        return await _dbContext.Permissions.AsNoTracking().ToListAsync();
    }

    public async Task<Permission?> GetPermissionByIdAsync(int id)
    {
        return await _dbContext.Permissions.SingleOrDefaultAsync(permission => permission.Id == id);
    }

    public async Task<Permission> AddPermissionAsync(Permission permission)
    {
        await _dbContext.Permissions.AddAsync(permission);
        await _dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task UpdatePermissionAsync(Permission permission, string name)
    {
        permission.Name = name;
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task DeletePermissionAsync(Permission permission)
    {
        _dbContext.Remove(permission);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsPermissionExistAsync(string name)
    {
        return await _dbContext.Permissions.AnyAsync(permission => permission.Name == name);
    }
}