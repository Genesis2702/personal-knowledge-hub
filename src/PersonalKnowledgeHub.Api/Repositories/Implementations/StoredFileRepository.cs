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
    
    public async Task<StoredFile?> GetStoredFileByResourceIdAsync(int resourceId, CancellationToken cancellationToken)
    {
        return await _dbContext.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.ResourceId == resourceId, cancellationToken);
    }

    public async Task<StoredFile?> GetStoredFileByStoredKeyAsync(string storedKey, CancellationToken cancellationToken)
    {
        return await _dbContext.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.StoredKey == storedKey, cancellationToken);
    }

    public async Task<StoredFile?> GetStoredFileByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, cancellationToken); 
    }

    public async Task<StoredFile> AddStoredFileAsync(StoredFile storedFile, CancellationToken cancellationToken)
    {
        await _dbContext.StoredFiles.AddAsync(storedFile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return storedFile;
    }

    public async Task DeleteStoredFileByStoredKeyAsync(string storedKey, CancellationToken cancellationToken)
    {
        await _dbContext.StoredFiles.Where(f => f.StoredKey == storedKey).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}