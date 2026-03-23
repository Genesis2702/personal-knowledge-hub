namespace PersonalKnowledgeHub.Entities
{
    public class Resource
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public required ResourceType ResourceType { get; set; }
        public required int UserId { get; set; }
        public User? User { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum ResourceType
    {
        Video,
        Article,
        Book,
        File
    }
}
