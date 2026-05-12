using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;
using PersonalKnowledgeHub.Mapper;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "ActiveAccount")]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;
        private readonly IDistributedCache _distributedCache;

        public ResourcesController(IResourceService resourceService, IDistributedCache distributedCache)
        {
            _resourceService = resourceService;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<ActionResult<PageResult<ResourceResponseDto>>> GetResources([FromQuery] ResourceQueryRequestDto resourceQueryRequest, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!resourceQueryRequest.TagId.HasValue && !resourceQueryRequest.ResourceType.HasValue && string.IsNullOrEmpty(resourceQueryRequest.Search))
            {
                string cacheKey = $"resource:{userId}:{resourceQueryRequest.PageIndex}:{resourceQueryRequest.PageSize}";
                string? cachedResources = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
                if (string.IsNullOrEmpty(cachedResources))
                {
                    PageResult<Resource> databaseResourcesPageResult = await _resourceService.GetResources(userId, resourceQueryRequest, cancellationToken);
                    cachedResources = JsonSerializer.Serialize(databaseResourcesPageResult);
                    DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(1)
                    };
                    await _distributedCache.SetStringAsync(cacheKey, cachedResources, cacheEntryOption, cancellationToken);
                }
                PageResult<Resource> resourcesPageResult = JsonSerializer.Deserialize<PageResult<Resource>>(cachedResources)!;
                PageResult<ResourceResponseDto> resourceResponsesPageResult = ResourceMapper.ToResourceResponsesPageResult(resourcesPageResult);
                return Ok(resourceResponsesPageResult);
            }
            else
            {
                PageResult<Resource> resourcesPageResult = await _resourceService.GetResources(userId, resourceQueryRequest, cancellationToken);
                PageResult<ResourceResponseDto> resourceResponsesPageResult = ResourceMapper.ToResourceResponsesPageResult(resourcesPageResult);
                return Ok(resourceResponsesPageResult);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceResponseDto>> GetResourceById(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string cacheKey = $"resource:{userId}:{id}";
            string? cachedResource = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (string.IsNullOrEmpty(cachedResource))
            {
                Resource databaseResource = await _resourceService.GetResourceById(id, userId, cancellationToken);
                cachedResource = JsonSerializer.Serialize(databaseResource);
                DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                };
                await _distributedCache.SetStringAsync(cacheKey, cachedResource, cacheEntryOption, cancellationToken);
            }
            Resource resource = JsonSerializer.Deserialize<Resource>(cachedResource)!;
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return Ok(resourceResponse);
        }

        [HttpPost]
        public async Task<ActionResult<ResourceResponseDto>> AddResource(ResourceRequestDto resourceRequest, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource = await _resourceService.AddResource(resourceRequest, userId, cancellationToken);
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return CreatedAtAction(nameof(GetResourceById), new { id = resource.Id }, resourceResponse);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateResourceById(ResourceUpdateRequestDto resourceUpdateRequest, int id, CancellationToken cancellationToken)
        {
            await _resourceService.UpdateResourceById(User, id, resourceUpdateRequest, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResourceById(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _resourceService.DeleteResourceById(User, id, cancellationToken);
            string cacheKey = $"resource:{userId}:{id}";
            await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<ResourceResponseDto>> RestoreResourceById(int id, CancellationToken cancellationToken)
        {
            Resource resource = await _resourceService.RestoreResourceById(User, id, cancellationToken);
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return Ok(resourceResponse);
        }
    }
}
