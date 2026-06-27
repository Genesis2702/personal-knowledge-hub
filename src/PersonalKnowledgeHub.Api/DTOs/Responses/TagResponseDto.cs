using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Responses
{
    public class TagResponseDto
    {
        [Required]
        public required string Name { get; set; }
        public DateTime LastModified { get; set; }
    }
}
