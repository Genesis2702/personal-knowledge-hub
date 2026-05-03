using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class UserQueryRequestDto
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public UserStatus? Status { get; set; }
}