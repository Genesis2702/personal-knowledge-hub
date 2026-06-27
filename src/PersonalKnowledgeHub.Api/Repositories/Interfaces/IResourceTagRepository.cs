using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IResourceTagRepository
    {
        public Task<ResourceTag> AddResourceTagAsync(ResourceTag resourceTag, CancellationToken cancellationToken);
        public Task<ResourceTag?> GetResourceTagByIdAsync(int tagId, int resourceId, CancellationToken cancellationToken);
        public Task DeleteResourceTagAsync(ResourceTag resourceTag, CancellationToken cancellationToken);
        public Task<bool> IsResourceTagExistAsync(int tagId, int resourceId, CancellationToken cancellationToken);
    }
}
