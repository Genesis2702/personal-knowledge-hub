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

        public async Task<ResourceTag> AddResourceTagAsync(ResourceTag resourceTag, CancellationToken cancellationToken)
        {
            await _dbContext.ResourceTags.AddAsync(resourceTag, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return resourceTag;
        }

        public async Task<ResourceTag?> GetResourceTagByIdAsync(int tagId, int resourceId, CancellationToken cancellationToken)
        {
            return await _dbContext.ResourceTags.
                Include(resourceTag => resourceTag.Resource).
                Include(resourceTag => resourceTag.Tag).
                FirstOrDefaultAsync(resourceTag => resourceTag.TagId == tagId && resourceTag.ResourceId == resourceId, cancellationToken);
        }

        public async Task DeleteResourceTagAsync(ResourceTag resourceTag, CancellationToken cancellationToken)
        {
            _dbContext.ResourceTags.Remove(resourceTag);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsResourceTagExistAsync(int tagId, int resourceId, CancellationToken cancellationToken)
        {
            return await _dbContext.ResourceTags.AnyAsync(resourceTag => resourceTag.TagId == tagId && resourceTag.ResourceId == resourceId, cancellationToken);
        }
    }
}
