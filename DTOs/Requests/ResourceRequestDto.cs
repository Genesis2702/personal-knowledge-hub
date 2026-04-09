using PersonalKnowledgeHub.Entities;
using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class ResourceRequestDto
    {
        [Required]
        public required string Title { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        [Required]
        public required ResourceType ResourceType { get; set; }
    }
}
