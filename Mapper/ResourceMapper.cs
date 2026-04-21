using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Mapper;

public class ResourceMapper
{
    public static Resource ToResource(ResourceRequestDto resourceRequest, int userId)
    {
        return new Resource
        {
            Title = resourceRequest.Title,
            Url = resourceRequest.Url,
            Description = resourceRequest.Description,
            ResourceType = resourceRequest.ResourceType,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static ResourceResponseDto ToResourceResponseDto(Resource resource)
    {
        return new ResourceResponseDto
        {
            Title = resource.Title,
            Url = resource.Url,
            Description = resource.Description,
            ResourceType = resource.ResourceType,
            CreatedAt = resource.CreatedAt,
            Tags = resource.ResourceTags.Select(resourceTag => resourceTag.Tag.Name).ToList()
        };
    }

    public static PageResult<Resource> ToResourcesPageResult(List<Resource> resources, int resourcesCount, int pageIndex, int pageSize)
    {
        return new PageResult<Resource>
        {
            Items = resources,
            PageIndex = pageIndex,
            PageSize = pageSize,
            PageCount = (int)Math.Ceiling((decimal)resourcesCount / pageSize)
        };
    }

    public static PageResult<ResourceResponseDto> ToResourceResponsesPageResult(PageResult<Resource> resourcesPageResult)
    {
        return new PageResult<ResourceResponseDto>
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
    }
}