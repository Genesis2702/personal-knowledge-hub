using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IResourceTagRepository
    {
        public Task<ResourceTag> AddResourceTagAsync(ResourceTag resourceTag);
        public Task<ResourceTag?> GetResourceTagByIdAsync(int tagId, int resourceId);
        public Task DeleteResourceTagAsync(ResourceTag resourceTag);
        public Task<bool> IsResourceTagExistAsync(int tagId, int resourceId);
    }
}
