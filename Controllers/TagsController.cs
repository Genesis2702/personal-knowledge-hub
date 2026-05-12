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

        [HttpPost]
        public async Task<ActionResult<TagResponseDto>> AddTag(TagRequestDto tagRequest, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Tag tag = await _tagService.AddTag(tagRequest, userId, cancellationToken);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tagResponse);
        }

        [HttpGet]
        public async Task<ActionResult<List<TagResponseDto>>> GetTags(CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            List<Tag> tags = await _tagService.GetTags(userId, cancellationToken);
            List<TagResponseDto> tagResponses = TagMapper.ToTagResponseList(tags);
            return Ok(tagResponses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagResponseDto>> GetTagById(int id, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Tag tag = await _tagService.GetTagById(id, userId, cancellationToken);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return Ok(tagResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTagById(TagRequestDto tagRequest, int id, CancellationToken cancellationToken)
        {
            await _tagService.UpdateTagById(User, tagRequest, id, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTagById(int id, CancellationToken cancellationToken)
        {
            await _tagService.DeleteTagById(User, id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<TagResponseDto>> RestoreTagById(int id, CancellationToken cancellationToken)
        {
            Tag tag = await _tagService.RestoreTagById(User, id, cancellationToken);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return Ok(tagResponse);
        }
    }
}
