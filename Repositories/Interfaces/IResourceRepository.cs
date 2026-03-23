using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IResourceRepository
    {
        public Task<List<Resource>> GetResourcesAsync(int userId);
        public Task<Resource?> GetResourceByIdAsync(int resourceId);
        public Task DeleteResourceAsync(Resource resource);
        public Task<Resource> AddResourceAsync(Resource resource);
        public Task<bool> IsTitleExistAsync(string resourceTitle, int userId);
    }
}
