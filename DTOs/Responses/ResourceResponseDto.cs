using PersonalKnowledgeHub.Entities;
using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Responses
{
    public class ResourceResponseDto
    {
        [Required]
        public required string Title { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        [Required]
        public required ResourceType ResourceType { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<string> Tags { get; set; } = new List<string>();
    }
}
