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
    [Authorize]
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
        public async Task<ActionResult<PageResult<ResourceResponseDto>>> GetResources([FromQuery] ResourceQueryRequestDto resourceQueryRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!resourceQueryRequest.TagId.HasValue && !resourceQueryRequest.ResourceType.HasValue && string.IsNullOrEmpty(resourceQueryRequest.Search))
            {
                string cacheKey = $"resource:{userId}:{resourceQueryRequest.PageIndex}:{resourceQueryRequest.PageSize}";
                string? cachedResources = await _distributedCache.GetStringAsync(cacheKey);
                if (string.IsNullOrEmpty(cachedResources))
                {
                    PageResult<Resource> databaseResourcesPageResult = await _resourceService.GetResources(userId, resourceQueryRequest);
                    cachedResources = JsonSerializer.Serialize(databaseResourcesPageResult);
                    DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(1)
                    };
                    await _distributedCache.SetStringAsync(cacheKey, cachedResources, cacheEntryOption);
                }
                PageResult<Resource> resourcesPageResult = JsonSerializer.Deserialize<PageResult<Resource>>(cachedResources)!;
                PageResult<ResourceResponseDto> resourceResponsesPageResult = ResourceMapper.ToResourceResponsesPageResult(resourcesPageResult);
                return Ok(resourceResponsesPageResult);
            }
            else
            {
                PageResult<Resource> resourcesPageResult = await _resourceService.GetResources(userId, resourceQueryRequest);
                PageResult<ResourceResponseDto> resourceResponsesPageResult = ResourceMapper.ToResourceResponsesPageResult(resourcesPageResult);
                return Ok(resourceResponsesPageResult);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceResponseDto>> GetResourceById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string cacheKey = $"resource:{userId}:{id}";
            string? cachedResource = await _distributedCache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(cachedResource))
            {
                Resource databaseResource = await _resourceService.GetResourceById(id, userId);
                cachedResource = JsonSerializer.Serialize(databaseResource);
                DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                };
                await _distributedCache.SetStringAsync(cacheKey, cachedResource, cacheEntryOption);
            }
            Resource resource = JsonSerializer.Deserialize<Resource>(cachedResource)!;
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return Ok(resourceResponse);
        }

        [HttpPost]
        public async Task<ActionResult<ResourceResponseDto>> AddResource(ResourceRequestDto resourceRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource = await _resourceService.AddResource(resourceRequest, userId);
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return CreatedAtAction(nameof(GetResourceById), new { id = resource.Id }, resourceResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResourceById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _resourceService.DeleteResourceById(id, userId);
            string cacheKey = $"resource:{userId}:{id}";
            await _distributedCache.RemoveAsync(cacheKey);
            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<ResourceResponseDto>> RestoreResourceById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource = await _resourceService.RestoreResourceById(id, userId);
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return Ok(resourceResponse);
        }
    }
}
