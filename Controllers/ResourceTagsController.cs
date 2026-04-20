using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;
using System.Security.Claims;
using PersonalKnowledgeHub.Mapper;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("resources/{resourceId}/tags")]
    [Authorize]
    public class ResourceTagsController : ControllerBase
    {
        private readonly IResourceTagService _resourceTagService;

        public ResourceTagsController(IResourceTagService resourceTagService)
        {
            _resourceTagService = resourceTagService;
        }

        [HttpPost]
        public async Task<ActionResult<ResourceResponseDto>> AddResourceTag(ResourceTagRequestDto resourceTagRequest, int resourceId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Resource resource = await _resourceTagService.AddResourceTag(resourceTagRequest.TagId, resourceId, userId);
            ResourceResponseDto resourceResponse = ResourceMapper.ToResourceResponseDto(resource);
            return CreatedAtAction(nameof(ResourcesController.GetResourceById), "Resources", new { id = resource.Id }, resourceResponse);
        }

        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteResourceTag(int tagId, int resourceId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _resourceTagService.DeleteResourceTag(tagId, resourceId, userId);
            return NoContent();
        }
    }
}
