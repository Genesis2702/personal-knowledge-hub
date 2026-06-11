using System.Security.Claims;
using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IResourceService
    {
        public Task<PageResult<Resource>> GetResources(int userId, ResourceQueryRequestDto resourceQueryRequest, CancellationToken cancellationToken);
        public Task<Resource> GetResourceById(int resourceId, int userId, CancellationToken cancellationToken);
        public Task<Resource> AddResource(ResourceRequestDto resourceRequest, int userId, CancellationToken cancellationToken);
        public Task UpdateResourceById(ClaimsPrincipal user, int resourceId, ResourceUpdateRequestDto resourceUpdateRequest, CancellationToken cancellationToken);
        public Task DeleteResourceById(ClaimsPrincipal user, int resourceId, CancellationToken cancellationToken);
        public Task CleanUpResources(CancellationToken cancellationToken);
        public Task<Resource> RestoreResourceById(ClaimsPrincipal user, int resourceId, CancellationToken cancellationToken);
    }
}
