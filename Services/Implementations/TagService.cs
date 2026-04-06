using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<Tag> AddTag(TagRequestDto tagRequest, int userId)
        {
            string tagName = tagRequest.Name.Trim().ToLower();
            if (await _tagRepository.IsTagExistAsync(tagName, userId))
            {
                throw new ConflictException("Tag already existed");
            }
            Tag tag = new Tag
            {
                Name = tagName,
                UserId = userId,
            };
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

        public async Task UpdateTagById(TagRequestDto tagRequest, int tagId, int userId)
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
            string tagName = tagRequest.Name.Trim().ToLower();
            if (await _tagRepository.IsTagExistAsync(tagName, userId))
            {
                throw new ConflictException("Tag already existed");
            }
            await _tagRepository.UpdateTagAsync(tagName, tag);
        }

        public async Task DeleteTagById(int tagId, int userId)
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
            await _tagRepository.DeleteTagAsync(tag);
        }
    }
}
