using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IResourceRepository
    {
        public Task<(List<Resource>, int)> GetResourcesAsync(int userId, int pageIndex, int pageSize, int? tagId, ResourceType? resourceType, string? search, CancellationToken cancellationToken);
        public Task<Resource?> GetResourceByIdAsync(int resourceId, CancellationToken cancellationToken);
        public Task<Resource?> GetResourceByIdForRestoreAsync(int resourceId, CancellationToken cancellationToken);
        public Task<Resource> AddResourceAsync(Resource resource, CancellationToken cancellationToken);
        public Task<int> UpdateResourceAsync(int resourceId, long version, string? title, string? url, string? description, CancellationToken cancellationToken);
        public Task DeleteResourceAsync(Resource resource, int userId, CancellationToken cancellationToken);
        public Task<Resource> RestoreResourceAsync(Resource resource, CancellationToken cancellationToken);
        public Task CleanUpResourcesAsync(CancellationToken cancellationToken);
        public Task<bool> IsTitleExistAsync(string resourceTitle, int userId, CancellationToken cancellationToken);
    }
}
