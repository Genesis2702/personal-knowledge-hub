using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IResourceTagService
    {
        public Task<Resource> AddResourceTag(int tagId, int resourceId, int userId, CancellationToken cancellationToken);
        public Task DeleteResourceTag(int tagId, int resourceId, int userId, CancellationToken cancellationToken);
    }
}
