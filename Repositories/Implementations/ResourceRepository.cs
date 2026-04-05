using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations
{
    public class ResourceRepository : IResourceRepository
    {
        private readonly AppDbContext _dbContext;

        public ResourceRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Resource>> GetResourcesAsync(int userId)
        {
            return await _dbContext.Resources.AsNoTracking()
                .Where(resource => resource.UserId == userId)
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .ToListAsync();
        }

        public async Task<Resource?> GetResourceByIdAsync(int resourceId)
        {
            return await _dbContext.Resources
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .SingleOrDefaultAsync(resource => resource.Id == resourceId);
        }

        public async Task<Resource> AddResourceAsync(Resource resource)
        {
            await _dbContext.Resources.AddAsync(resource);
            await _dbContext.SaveChangesAsync();
            return resource;
        }

        public async Task DeleteResourceAsync(Resource resource)
        {
            _dbContext.Resources.Remove(resource);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Resource>> FilterResourcesByTagAsync(int tagId, int userId)
        {
            return await _dbContext.Resources.Where(resource => resource.UserId == userId)
                .Where(resource => resource.ResourceTags.Any(resourceTag => resourceTag.TagId == tagId))
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .ToListAsync();
        }

        public async Task<bool> IsTitleExistAsync(string resourceTitle, int userId)
        {
            return await _dbContext.Resources.AnyAsync(resource => resource.Title == resourceTitle && resource.UserId == userId);
        }
    }
}
