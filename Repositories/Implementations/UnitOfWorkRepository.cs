using Microsoft.EntityFrameworkCore.Storage;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations;

public class UnitOfWorkRepository : IUnitOfWorkRepository
{
    private readonly AppDbContext _dbContext;

    public UnitOfWorkRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync();
    }
}