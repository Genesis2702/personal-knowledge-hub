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

        public async Task<ResourceTag> AddResourceTag(int tagId, int resourceId, int userId)
        {
            if (await _resourceTagRepository.IsResourceTagExistAsync(tagId, resourceId))
            {
                throw new ConflictException("Resource tag already exited");
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
            return await _resourceTagRepository.AddResourceTagAsync(resourceTag);
        }

        public async Task<List<Resource>> FilterResourceTag(int tagId, int userId)
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
            return await _resourceTagRepository.FilterResourceTagAsync(tagId, userId);
        }
    }
}
