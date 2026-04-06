using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IResourceTagService
    {
        public Task<Resource> AddResourceTag(int tagId, int resourceId, int userId);
        public Task DeleteResourceTag(int tagId, int resourceId, int userId);
    }
}
