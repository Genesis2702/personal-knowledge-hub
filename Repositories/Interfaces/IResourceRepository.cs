using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IResourceRepository
    {
        public Task<List<Resource>> GetResourcesAsync(int userId, int pageIndex, int pageSize, int? tagId, string? search);
        public Task<Resource?> GetResourceByIdAsync(int resourceId);
        public Task<int> GetResourcesCount(int userId, int? tagId, string? search);
        public Task<Resource> AddResourceAsync(Resource resource);
        public Task DeleteResourceAsync(Resource resource);
        public Task<bool> IsTitleExistAsync(string resourceTitle, int userId);
    }
}
