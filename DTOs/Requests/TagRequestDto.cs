using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class TagRequestDto
    {
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }
    }
}
