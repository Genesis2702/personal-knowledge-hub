using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IAuthorizationService _authorizationService;

        public ResourceService(IResourceRepository resourceRepository, ITagRepository tagRepository, IAuthorizationService authorizationService)
        {
            _resourceRepository = resourceRepository;
            _tagRepository = tagRepository;
            _authorizationService = authorizationService;
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

        public async Task UpdateResourceById(ClaimsPrincipal user, int resourceId, ResourceUpdateRequestDto resourceUpdateRequest)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("You are not authorized to update this resource");
            }
            await _resourceRepository.UpdateResourceAsync(resource, resourceUpdateRequest.Title,
                resourceUpdateRequest.Url, resourceUpdateRequest.Description);
        }

        public async Task DeleteResourceById(ClaimsPrincipal user, int resourceId)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("You are not authorized to delete this resource");
            }
            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _resourceRepository.DeleteResourceAsync(resource, userId);
        }

        public async Task<Resource> RestoreResourceById(ClaimsPrincipal user, int resourceId)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdForRestoreAsync(resourceId);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("You are not authorized to restore this resource");
            }
            return await _resourceRepository.RestoreResourceAsync(resource);
        }
    }
}
