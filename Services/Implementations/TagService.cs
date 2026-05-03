using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Mapper;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IAuthorizationService _authorizationService;

        public TagService(ITagRepository tagRepository, IAuthorizationService authorizationService)
        {
            _tagRepository = tagRepository;
            _authorizationService = authorizationService;
        }

        public async Task<Tag> AddTag(TagRequestDto tagRequest, int userId)
        {
            Tag tag = TagMapper.ToTag(tagRequest, userId);
            if (await _tagRepository.IsTagExistAsync(tag.Name, userId))
            {
                throw new ConflictException("Tag already existed");
            }
            return await _tagRepository.AddTagAsync(tag);
        }

        public async Task<List<Tag>> GetTags(int userId)
        {
            return await _tagRepository.GetTagsAsync(userId);
        }

        public async Task<Tag> GetTagById(int tagId, int userId)
        {
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
            {
                throw new NotFoundException("Tag not found");
            }
            if (tag.UserId != userId)
            {
                throw new ForbiddenException("Tag found doesn't belong to current user");
            }
            return tag;
        }

        public async Task UpdateTagById(ClaimsPrincipal user, TagRequestDto tagRequest, int tagId)
        {
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
            {
                throw new NotFoundException("Tag not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, tag, "TagOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("You are not authorized to update this tag");
            }
            string tagName = tagRequest.Name.Trim().ToLower();
            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (await _tagRepository.IsTagExistAsync(tagName, userId))
            {
                throw new ConflictException("Tag already existed");
            }
            await _tagRepository.UpdateTagAsync(tagName, tag);
        }

        public async Task DeleteTagById(ClaimsPrincipal user, int tagId)
        {
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
            {
                throw new NotFoundException("Tag not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, tag, "TagOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("You are not authorized to delete this tag");
            }
            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _tagRepository.DeleteTagAsync(tag, userId);
        }

        public async Task<Tag> RestoreTagById(ClaimsPrincipal user, int tagId)
        {
            Tag? tag = await _tagRepository.GetTagByIdForRestoreAsync(tagId);
            if (tag == null)
            {
                throw new NotFoundException("Tag not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, tag, "TagOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("Tag found doesn't belong to current user");
            }
            return await _tagRepository.RestoreTagAsync(tag);
        }
    }
}
