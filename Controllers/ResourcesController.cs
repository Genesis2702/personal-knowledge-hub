using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;
using System.Security.Claims;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourcesController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        public async Task<ActionResult<PageResult<ResourceResponseDto>>> GetResources([FromQuery] ResourceQueryRequestDto resourceQueryRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            PageResult<Resource> resourcesPageResult = await
                _resourceService.GetResources(userId,
                resourceQueryRequest.PageIndex,
                resourceQueryRequest.PageSize,
                resourceQueryRequest.TagId,
                resourceQueryRequest.ResourceType,
                resourceQueryRequest.Search);
            PageResult<ResourceResponseDto> resourceResponsesPageResult = new PageResult<ResourceResponseDto>
            {
                Items = resourcesPageResult.Items.Select(item => new ResourceResponseDto
                {
                    Title = item.Title,
                    Url = item.Url,
                    Description = item.Description,
                    ResourceType = item.ResourceType,
                    CreatedAt = item.CreatedAt,
                    Tags = item.ResourceTags.Select(resourceTag => resourceTag.Tag.Name).ToList()
                }).ToList(),
                PageIndex = resourcesPageResult.PageIndex,
                PageSize = resourcesPageResult.PageSize,
                PageCount = resourcesPageResult.PageCount
            };
            return Ok(resourceResponsesPageResult);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceResponseDto>> GetResourceById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource = await _resourceService.GetResourceById(id, userId);
            ResourceResponseDto resourceResponse = new ResourceResponseDto
            {
                Title = resource.Title,
                Url = resource.Url,
                Description = resource.Description,
                ResourceType = resource.ResourceType,
                CreatedAt = resource.CreatedAt,
                Tags = resource.ResourceTags.Select(resourceTag => resourceTag.Tag.Name).ToList()
            };
            return Ok(resourceResponse);
        }

        [HttpPost]
        public async Task<ActionResult<ResourceResponseDto>> AddResource(ResourceRequestDto resourceRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource = await _resourceService.AddResource(resourceRequest, userId);
            ResourceResponseDto resourceResponse = new ResourceResponseDto
            {
                Title = resource.Title,
                Url = resource.Url,
                Description = resource.Description,
                ResourceType = resource.ResourceType,
                CreatedAt = resource.CreatedAt,
            };
            return CreatedAtAction(nameof(GetResourceById), new { id = resource.Id }, resourceResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResourceById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _resourceService.DeleteResourceById(id, userId);
            return NoContent();
        }
    }
}
