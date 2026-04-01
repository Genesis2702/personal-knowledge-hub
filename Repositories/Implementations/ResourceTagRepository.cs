using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations
{
    public class ResourceTagRepository : IResourceTagRepository
    {
        private readonly AppDbContext _dbContext;

        public ResourceTagRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResourceTag> AddResourceTagAsync(ResourceTag resourceTag)
        {
            await _dbContext.ResourceTags.AddAsync(resourceTag);
            await _dbContext.SaveChangesAsync();
            return resourceTag;
        }

        public async Task<List<Resource>> FilterResourceTagAsync(int tagId, int userId)
        {
            return await _dbContext.Resources.Where(resource => resource.UserId == userId)
                .Where(resource => resource.ResourceTags.Any(resourceTag => resourceTag.TagId == tagId))
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .ToListAsync();
        }

        public async Task<bool> IsResourceTagExistAsync(int tagId, int resourceId)
        {
            return await _dbContext.ResourceTags.AnyAsync(resourceTag => resourceTag.TagId == tagId && resourceTag.ResourceId == resourceId);
        }
    }
}
