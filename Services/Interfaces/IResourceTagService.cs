using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IResourceTagService
    {
        public Task<ResourceTag> AddResourceTag(int tagId, int resourceId, int userId);
        public Task<List<Resource>> FilterResourceTag(int tagId, int userId);
    }
}
