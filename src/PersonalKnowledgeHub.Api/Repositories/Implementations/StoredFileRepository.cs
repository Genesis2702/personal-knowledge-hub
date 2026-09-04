using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations;

public class StoredFileRepository : IStoredFileRepository
{
    private readonly AppDbContext _dbContext;

    public StoredFileRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<StoredFile?> GetStoredFileByResourceId(int resourceId)
    {
        return await _dbContext.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.ResourceId == resourceId);
    }

    public async Task<StoredFile?> GetStoredFileByStoredKey(string storedKey)
    {
        return await _dbContext.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.StoredKey == storedKey);
    }

    public async Task<StoredFile?> GetStoredFileById(int id)
    {
        return await _dbContext.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id); 
    }

    public async Task<StoredFile> AddStoredFile(StoredFile storedFile)
    {
        await _dbContext.StoredFiles.AddAsync(storedFile);
        await _dbContext.SaveChangesAsync();
        return storedFile;
    }

    public async Task DeleteStoredFileByStoredKey(string storedKey)
    {
        await _dbContext.StoredFiles.Where(f => f.StoredKey == storedKey).ExecuteDeleteAsync();
        await _dbContext.SaveChangesAsync();
    }
}