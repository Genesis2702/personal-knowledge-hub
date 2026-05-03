using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Mapper;

public class TagMapper
{
    public static Tag ToTag(TagRequestDto tagRequest, int userId)
    {
        return new Tag
        {
            Name = tagRequest.Name.Trim().ToLower(),
            UserId = userId,
            LastModified = DateTime.UtcNow,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };
    }

    public static TagResponseDto ToTagResponseDto(Tag tag)
    {
        return new TagResponseDto
        {
            Name = tag.Name,
            LastModified = tag.LastModified
        };
    }
    
    public static List<TagResponseDto> ToTagResponseList(List<Tag> tags)
    {
        return tags.Select(tag => new TagResponseDto
        {
            Name = tag.Name,
            LastModified = tag.LastModified
        }).ToList();
    }
}