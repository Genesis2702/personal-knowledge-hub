using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IResourceService
    {
        public Task<List<Resource>> GetResources(int userId);
        public Task<Resource> GetResourceById(int resourceId, int userId);
        public Task DeleteResourceById(int resourceId, int userId);
        public Task<Resource> AddResource(Resource resource);
    }
}
