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

    public async Task<List<Permission>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Permissions.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Permission?> GetPermissionByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Permissions.SingleOrDefaultAsync(permission => permission.Id == id, cancellationToken);
    }

    public async Task<Permission> AddPermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        await _dbContext.Permissions.AddAsync(permission, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return permission;
    }

    public async Task UpdatePermissionAsync(Permission permission, string name, CancellationToken cancellationToken)
    {
        permission.Name = name;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeletePermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        _dbContext.Remove(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsPermissionExistAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Permissions.AnyAsync(permission => permission.Name == name, cancellationToken);
    }
}