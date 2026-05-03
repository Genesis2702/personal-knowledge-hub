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
        public async Task<ActionResult<TagResponseDto>> AddTag(TagRequestDto tagRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Tag tag = await _tagService.AddTag(tagRequest, userId);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tagResponse);
        }

        [HttpGet]
        public async Task<ActionResult<List<TagResponseDto>>> GetTags()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            List<Tag> tags = await _tagService.GetTags(userId);
            List<TagResponseDto> tagResponses = TagMapper.ToTagResponseList(tags);
            return Ok(tagResponses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagResponseDto>> GetTagById(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Tag tag = await _tagService.GetTagById(id, userId);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return Ok(tagResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTagById(TagRequestDto tagRequest, int id)
        {
            await _tagService.UpdateTagById(User, tagRequest, id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTagById(int id)
        {
            await _tagService.DeleteTagById(User, id);
            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<TagResponseDto>> RestoreTagById(int id)
        {
            Tag tag = await _tagService.RestoreTagById(User, id);
            TagResponseDto tagResponse = TagMapper.ToTagResponseDto(tag);
            return Ok(tagResponse);
        }
    }
}
