using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Responses
{
    public class TagResponseDto
    {
        [Required]
        public required string Name { get; set; }
    }
}
