using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;
using PersonalKnowledgeHub.Mapper;
using PersonalKnowledgeHub.Models;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "ActiveAccount")]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;
        private readonly IFileResourceService _fileResourceService;
        private readonly IDistributedCache _distributedCache;

        public ResourcesController(IResourceService resourceService, IDistributedCache distributedCache, IFileResourceService fileResourceService)
        {
            _resourceService = resourceService;
            _distributedCache = distributedCache;
            _fileResourceService = fileResourceService;
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
                    PageResult<Resource> resourcesPageResult = await _resourceService.GetResources(userId, resourceQueryRequest, cancellationToken);
                    PageResult<ResourceResponseDto> resourceResponsesPageResult = ResourceMapper.ToResourceResponsesPageResult(resourcesPageResult);
                    cachedResources = JsonSerializer.Serialize(resourceResponsesPageResult);
                    DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(1)
                    };
                    await _distributedCache.SetStringAsync(cacheKey, cachedResources, cacheEntryOption, cancellationToken);
                }
                PageResult<ResourceResponseDto> response = JsonSerializer.Deserialize<PageResult<ResourceResponseDto>>(cachedResources)!;
                return Ok(response);
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
                Resource resource = await _resourceService.GetResourceById(id, userId, cancellationToken);
                ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
                cachedResource = JsonSerializer.Serialize(resourceResponse);
                DistributedCacheEntryOptions cacheEntryOption = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
                };
                await _distributedCache.SetStringAsync(cacheKey, cachedResource, cacheEntryOption, cancellationToken);
            }
            ResourceResponseDto response = JsonSerializer.Deserialize<ResourceResponseDto>(cachedResource)!;
            return Ok(response);
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

        [HttpPost("files")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResourceResponseDto>> UploadFile([FromForm] FileUploadRequestDto fileUploadRequest,
            CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource =
                await _fileResourceService.CreateFileResource(fileUploadRequest.FormFile, userId, cancellationToken);
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return CreatedAtAction(nameof(GetResourceById), new { id = resource.Id }, resourceResponse);
        }

        [HttpGet("{id}/file")]
        public async Task<IActionResult> PreviewFile(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            FileDownloadResult result = await _fileResourceService.OpenFileResource(id, userId, cancellationToken);
            return File(result.Content, result.ContentType);
        }

        [HttpGet("{id}/file/download")]
        public async Task<IActionResult> DownloadFile(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            FileDownloadResult result = await _fileResourceService.OpenFileResource(id, userId, cancellationToken);
            return File(result.Content, result.ContentType, result.FileName);
        }
    }
}
