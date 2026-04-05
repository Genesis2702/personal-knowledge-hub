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

        public async Task<ResourceTag?> GetResourceTagByIdAsync(int tagId, int resourceId)
        {
            return await _dbContext.ResourceTags.
                Include(resourceTag => resourceTag.Resource).
                Include(resourceTag => resourceTag.Tag).
                FirstOrDefaultAsync(resourceTag => resourceTag.TagId == tagId && resourceTag.ResourceId == resourceId);
        }

        public async Task DeleteResourceTagAsync(ResourceTag resourceTag)
        {
            _dbContext.ResourceTags.Remove(resourceTag);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsResourceTagExistAsync(int tagId, int resourceId)
        {
            return await _dbContext.ResourceTags.AnyAsync(resourceTag => resourceTag.TagId == tagId && resourceTag.ResourceId == resourceId);
        }
    }
}
