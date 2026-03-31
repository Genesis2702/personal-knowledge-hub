using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IResourceTagRepository
    {
        public Task<ResourceTag> AddResourceTagAsync(ResourceTag resourceTag);
        public Task<List<Resource>> FilterResourceTagAsync(int tagId, int userId);
    }
}
