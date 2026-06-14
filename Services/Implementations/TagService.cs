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
        private readonly ILogger<TagService> _logger;

        public TagService(ITagRepository tagRepository, IAuthorizationService authorizationService, ILogger<TagService> logger)
        {
            _tagRepository = tagRepository;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task<Tag> AddTag(TagRequestDto tagRequest, int userId, CancellationToken cancellationToken)
        {
            Tag tag = TagMapper.ToTag(tagRequest, userId);
            if (await _tagRepository.IsTagExistAsync(tag.Name, userId, cancellationToken))
            {
                throw new ConflictException("Tag already existed");
            }
            return await _tagRepository.AddTagAsync(tag, cancellationToken);
        }

        public async Task<List<Tag>> GetTags(int userId, CancellationToken cancellationToken)
        {
            return await _tagRepository.GetTagsAsync(userId, cancellationToken);
        }

        public async Task<Tag> GetTagById(int tagId, int userId, CancellationToken cancellationToken)
        {
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId, cancellationToken);
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

        public async Task UpdateTagById(ClaimsPrincipal user, TagRequestDto tagRequest, int tagId, CancellationToken cancellationToken)
        {
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId, cancellationToken);
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
            if (await _tagRepository.IsTagExistAsync(tagName, userId, cancellationToken))
            {
                throw new ConflictException("Tag already existed");
            }
            int updatedRows = await _tagRepository.UpdateTagAsync(tagId, tag.Version, tagName, cancellationToken);
            if (updatedRows == 0)
            {
                throw new ConflictException("Tag has been updated by another user");
            }
        }

        public async Task DeleteTagById(ClaimsPrincipal user, int tagId, CancellationToken cancellationToken)
        {
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId, cancellationToken);
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
            await _tagRepository.DeleteTagAsync(tag, userId, cancellationToken);
        }

        public async Task CleanUpTags(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tags cleaning up started");
            await _tagRepository.CleanUpTagsAsync(cancellationToken);
            _logger.LogInformation("Tags cleaned up successfully");
        }

        public async Task<Tag> RestoreTagById(ClaimsPrincipal user, int tagId, CancellationToken cancellationToken)
        {
            Tag? tag = await _tagRepository.GetTagByIdForRestoreAsync(tagId, cancellationToken);
            if (tag == null)
            {
                throw new NotFoundException("Tag not found");
            }
            var result = await _authorizationService.AuthorizeAsync(user, tag, "TagOwnerOrAdmin");
            if (!result.Succeeded)
            {
                throw new ForbiddenException("Tag found doesn't belong to current user");
            }
            return await _tagRepository.RestoreTagAsync(tag, cancellationToken);
        }
    }
}
