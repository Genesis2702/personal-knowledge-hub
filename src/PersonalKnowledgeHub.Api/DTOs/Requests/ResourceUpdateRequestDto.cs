using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class ResourceUpdateRequestDto
{
    [MinLength(1)]
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Description { get; set; }
}