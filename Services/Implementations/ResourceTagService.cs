using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class ResourceTagService : IResourceTagService
    {
        private readonly IResourceTagRepository _resourceTagRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly ITagRepository _tagRepository;

        public ResourceTagService(IResourceTagRepository resourceTagRepository, IResourceRepository resourceRepository, ITagRepository tagRepository)
        {
            _resourceTagRepository = resourceTagRepository;
            _resourceRepository = resourceRepository;
            _tagRepository = tagRepository;
        }

        public async Task<Resource> AddResourceTag(int tagId, int resourceId, int userId)
        {
            if (await _resourceTagRepository.IsResourceTagExistAsync(tagId, resourceId))
            {
                throw new ConflictException("Resource tag already existed");
            }
            Tag? tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null)
            {
                throw new NotFoundException("Tag not found");
            }
            if (tag.UserId != userId)
            {
                throw new ForbiddenException("Tag found doesn't belong to current user");
            }
            Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId);
            if (resource == null)
            {
                throw new NotFoundException("Resource not found");
            }
            if (resource.UserId != userId)
            {
                throw new ForbiddenException("Resource found doesn't belong to current user");
            }
            ResourceTag resourceTag = new ResourceTag
            {
                Tag = tag,
                TagId = tagId,
                Resource = resource,
                ResourceId = resourceId
            };
            await _resourceTagRepository.AddResourceTagAsync(resourceTag);
            return resource;
        }

        public async Task DeleteResourceTag(int tagId, int resourceId, int userId)
        {
            ResourceTag? resourceTag = await _resourceTagRepository.GetResourceTagByIdAsync(tagId, resourceId);
            if (resourceTag == null)
            {
                throw new NotFoundException("Resource's tag not found");
            }
            if (resourceTag.Tag.UserId != userId || resourceTag.Resource.UserId != userId)
            {
                throw new ForbiddenException("Resource's tag found doesn't belong to current user");
            }
            await _resourceTagRepository.DeleteResourceTagAsync(resourceTag);
        }
    }
}
