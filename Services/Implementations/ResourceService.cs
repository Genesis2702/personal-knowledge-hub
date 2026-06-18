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
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(IResourceRepository resourceRepository, ITagRepository tagRepository, 
            IAuthorizationService authorizationService, ILogger<ResourceService> logger)
        {
            _resourceRepository = resourceRepository;
            _tagRepository = tagRepository;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task<PageResult<Resource>> GetResources(int userId, ResourceQueryRequestDto resourceQueryRequest, CancellationToken cancellationToken)
        {
            if (resourceQueryRequest.TagId.HasValue)
            {
                Tag? tag = await _tagRepository.GetTagByIdAsync(resourceQueryRequest.TagId.Value, cancellationToken);
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
                    resourceQueryRequest.Search,
                    cancellationToken
                );
            return ResourceMapper.ToResourcesPageResult(resources, resourcesCount, resourceQueryRequest.PageIndex, resourceQueryRequest.PageSize);
        }

        public async Task<Resource> GetResourceById(int resourceId, int userId, CancellationToken cancellationToken)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId, cancellationToken);
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

        public async Task<Resource> AddResource(ResourceRequestDto resourceRequest, int userId, CancellationToken cancellationToken)
        {
            if (await _resourceRepository.IsTitleExistAsync(resourceRequest.Title, userId, cancellationToken))
            {
                throw new ConflictException("Title already existed");
            }
            Resource resource = ResourceMapper.ToResource(resourceRequest, userId);
            Resource addedResource = await _resourceRepository.AddResourceAsync(resource, cancellationToken);
            _logger.LogInformation("Resource {ResourceId} added successfully for user {UserId}", 
                addedResource.Id, userId);
            return addedResource;
        }

        public async Task UpdateResourceById(ClaimsPrincipal user, int resourceId, ResourceUpdateRequestDto resourceUpdateRequest, CancellationToken cancellationToken)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId, cancellationToken);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("You are not authorized to update this resource");
            }
            int updatedRows = await _resourceRepository.UpdateResourceAsync(resourceId, resource.Version, resourceUpdateRequest.Title,
                resourceUpdateRequest.Url, resourceUpdateRequest.Description, cancellationToken);
            if (updatedRows == 0)
            {
                throw new ConflictException("Resource has been updated by another user");
            }
            _logger.LogInformation("Resource {ResourceId} updated successfully for user {UserId}", resource.Id, user.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public async Task DeleteResourceById(ClaimsPrincipal user, int resourceId, CancellationToken cancellationToken)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId, cancellationToken);
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
            await _resourceRepository.DeleteResourceAsync(resource, userId, cancellationToken);
            _logger.LogInformation("Resource {ResourceId} deleted successfully for user {UserId}", resource.Id, userId);
        }

        public async Task CleanUpResources(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resources cleaning up started");
            try
            {
                await _resourceRepository.CleanUpResourcesAsync(cancellationToken);
                _logger.LogInformation("Resources cleaned up successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resources cleaning up failed");
                throw;
            }
        }

        public async Task<Resource> RestoreResourceById(ClaimsPrincipal user, int resourceId, CancellationToken cancellationToken)
        {
            Resource? resource = await _resourceRepository.GetResourceByIdForRestoreAsync(resourceId, cancellationToken);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, resource, "ResourceOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("You are not authorized to restore this resource");
            }
            Resource restoredResource = await _resourceRepository.RestoreResourceAsync(resource, cancellationToken);
            _logger.LogInformation("Resource {ResourceId} restored successfully for user {UserId}", 
                restoredResource.Id, user.FindFirstValue(ClaimTypes.NameIdentifier));
            return restoredResource;
        }
    }
}
