namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class ResourceQueryRequestDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? TagId { get; set; }
        public string? Search { get; set; }
    }
}
