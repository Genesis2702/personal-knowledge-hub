using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly ITagRepository _tagRepository;

        public ResourceService(IResourceRepository resourceRepository, ITagRepository tagRepository)
        {
            _resourceRepository = resourceRepository;
            _tagRepository = tagRepository;
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

        public async Task<Resource> AddResource(ResourceRequestDto resourceRequest, int userId)
        {
            if (await _resourceRepository.IsTitleExistAsync(resourceRequest.Title, userId))
            {
                throw new ConflictException("Title already existed");
            }
            Resource resource = new Resource
            {
                Title = resourceRequest.Title,
                Url = resourceRequest.Url,
                Description = resourceRequest.Description,
                ResourceType = resourceRequest.ResourceType,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            return await _resourceRepository.AddResourceAsync(resource);
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

        public async Task<List<Resource>> FilterResourcesByTag(int tagId, int userId)
        {
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
            {
                throw new NotFoundException("Tag not found");
            }
            if (tag.UserId != userId)
            {
                throw new ForbiddenException("Tag found doesn't belong to current user");
            }
            return await _resourceRepository.FilterResourcesByTagAsync(tagId, userId);
        }
    }
}
