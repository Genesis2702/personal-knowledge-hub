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
    [Route("[controller]")]
    [Authorize(Policy = "ActiveAccount")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [EndpointSummary("Create a new tag for the current user")]
        [HttpPost]
        public async Task<ActionResult<TagResponseDto>> AddTag(TagRequestDto tagRequest, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Tag tag = await _tagService.AddTag(tagRequest, userId, cancellationToken);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tagResponse);
        }

        [EndpointSummary("Get the current user's tags")]
        [HttpGet]
        public async Task<ActionResult<List<TagResponseDto>>> GetTags(CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            List<Tag> tags = await _tagService.GetTags(userId, cancellationToken);
            List<TagResponseDto> tagResponses = TagMapper.ToTagResponseList(tags);
            return Ok(tagResponses);
        }

        [EndpointSummary("Get one of the current user's tag by ID")]
        [HttpGet("{id}")]
        public async Task<ActionResult<TagResponseDto>> GetTagById(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Tag tag = await _tagService.GetTagById(id, userId, cancellationToken);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return Ok(tagResponse);
        }

        [EndpointSummary("Update one of the current user's tag by ID")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTagById(TagRequestDto tagRequest, int id, CancellationToken cancellationToken)
        {
            await _tagService.UpdateTagById(User, tagRequest, id, cancellationToken);
            return NoContent();
        }

        [EndpointSummary("Soft delete one of the current user's tag by ID")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTagById(int id, CancellationToken cancellationToken)
        {
            await _tagService.DeleteTagById(User, id, cancellationToken);
            return NoContent();
        }

        [EndpointSummary("Restore one of the current user's soft-deleted tag by ID")]
        [HttpPost("{id}/restore")]
        public async Task<ActionResult<TagResponseDto>> RestoreTagById(int id, CancellationToken cancellationToken)
        {
            Tag tag = await _tagService.RestoreTagById(User, id, cancellationToken);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return Ok(tagResponse);
        }
    }
}
