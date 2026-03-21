using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class ResourceRequestDto
    {
        public required string Title { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public required ResourceType ResourceType { get; set; }
    }
}
