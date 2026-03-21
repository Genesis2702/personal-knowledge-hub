using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _resourceRepository;

        public ResourceService(IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<List<Resource>> GetResources(int userId)
        {
            return await _resourceRepository.GetResourcesAsync(userId);
        }

        public async Task<Resource> GetResourceById(int resourceId, int userId)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            if (resource.UserId != userId)
            {
                throw new ForbiddenException("Resource found doesn't belong to current user");
            }
            return resource;
        }

        public async Task DeleteResourceById(int resourceId, int userId)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            if (resource.UserId != userId)
            {
                throw new ForbiddenException("Resource found doesn't belong to current user");
            }
            await _resourceRepository.DeleteResourceAsync(resource);
        }

        public async Task<Resource> AddResource(Resource resource)
        {
            if (await _resourceRepository.IsTitleExistAsync(resource.Title, resource.UserId))
            {
                throw new ConflictException("Title already existed");
            }
            return await _resourceRepository.AddResourceAsync(resource);
        }
    }
}
