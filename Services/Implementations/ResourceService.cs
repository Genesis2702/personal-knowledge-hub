using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Mapper;

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

        public async Task<PageResult<Resource>> GetResources(int userId, ResourceQueryRequestDto resourceQueryRequest)
        {
            if (resourceQueryRequest.TagId.HasValue)
            {
                Tag? tag = await _tagRepository.GetTagByIdAsync(resourceQueryRequest.TagId.Value);
                if (tag == null)
                {
                    throw new NotFoundException("Tag not found");
                }
                if (tag.UserId != userId)
                {
                    throw new ForbiddenException("Tag found doesn't belong to current user");
                }
            }
            (List<Resource> resources, int resourcesCount) = await _resourceRepository.GetResourcesAsync
                (
                    userId, 
                    resourceQueryRequest.PageIndex, 
                    resourceQueryRequest.PageSize, 
                    resourceQueryRequest.TagId, 
                    resourceQueryRequest.ResourceType, 
                    resourceQueryRequest.Search
                );
            return ResourceMapper.ToResourcesPageResult(resources, resourcesCount, resourceQueryRequest.PageIndex, resourceQueryRequest.PageSize);
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
            Resource resource = ResourceMapper.ToResource(resourceRequest, userId);
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
    }
}
