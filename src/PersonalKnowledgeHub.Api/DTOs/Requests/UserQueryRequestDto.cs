using System.ComponentModel.DataAnnotations;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class UserQueryRequestDto
{
    [Range(1, int.MaxValue)]
    public int PageIndex { get; set; } = 1;
    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 10;
    public UserStatus? Status { get; set; }
}