using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IResourceRepository
    {
        public Task<(List<Resource>, int)> GetResourcesAsync(int userId, int pageIndex, int pageSize, int? tagId, ResourceType? resourceType, string? search);
        public Task<Resource?> GetResourceByIdAsync(int resourceId);
        public Task<Resource?> GetResourceByIdForRestoreAsync(int resourceId);
        public Task<Resource> AddResourceAsync(Resource resource);
        public Task UpdateResourceAsync(Resource resource, string? title, string? url, string? description);
        public Task DeleteResourceAsync(Resource resource, int userId);
        public Task<Resource> RestoreResourceAsync(Resource resource);
        public Task<bool> IsTitleExistAsync(string resourceTitle, int userId);
    }
}
