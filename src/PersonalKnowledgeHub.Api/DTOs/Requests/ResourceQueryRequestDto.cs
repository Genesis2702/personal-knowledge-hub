using System.ComponentModel.DataAnnotations;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class ResourceQueryRequestDto
    {
        [Range(1, int.MaxValue)]
        public int PageIndex { get; set; } = 1;
        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = 10;
        public int? TagId { get; set; }
        public ResourceType? ResourceType { get; set; }
        [MaxLength(255)]
        public string? Search { get; set; }
    }
}
