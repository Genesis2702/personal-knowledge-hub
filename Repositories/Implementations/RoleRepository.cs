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

    public async Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Roles.AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<Role?> GetRoleByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Roles.SingleOrDefaultAsync(role => role.Id == id, cancellationToken);
    }

    public async Task<Role> AddRoleAsync(Role role, CancellationToken cancellationToken)
    {
        await _dbContext.Roles.AddAsync(role, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task UpdateRoleAsync(Role role, string name, CancellationToken cancellationToken)
    {
        role.Name = name;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRoleAsync(Entities.Role role, CancellationToken cancellationToken)
    {
        _dbContext.Remove(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsRoleExistAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbContext.Roles.AnyAsync(role => role.Name == name, cancellationToken);
    }

    public async Task<Role> AddPermissionToRoleAsync(RolePermission rolePermission, CancellationToken cancellationToken)
    {
        await _dbContext.RolePermissions.AddAsync(rolePermission, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return rolePermission.Role;
    }

    public async Task RemovePermissionFromRoleAsync(RolePermission rolePermission, CancellationToken cancellationToken)
    {
        _dbContext.RolePermissions.Remove(rolePermission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}