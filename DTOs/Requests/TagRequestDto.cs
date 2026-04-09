using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class TagRequestDto
    {
        [Required]
        public required string Name { get; set; }
    }
}
