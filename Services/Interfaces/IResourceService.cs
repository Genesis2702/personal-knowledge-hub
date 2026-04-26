using System.Security.Claims;
using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IResourceService
    {
        public Task<PageResult<Resource>> GetResources(int userId, ResourceQueryRequestDto resourceQueryRequest);
        public Task<Resource> GetResourceById(int resourceId, int userId);
        public Task<Resource> AddResource(ResourceRequestDto resourceRequest, int userId);
        public Task DeleteResourceById(ClaimsPrincipal user, int resourceId);
        public Task<Resource> RestoreResourceById(ClaimsPrincipal user, int resourceId);
    }
}
