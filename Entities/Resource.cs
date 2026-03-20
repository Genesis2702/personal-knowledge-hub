namespace PersonalKnowledgeHub.Entities
{
    public class Resource
    {
        public int Id { get; set; }
        public required String Title { get; set; }
        public String? Url { get; set; }
        public String? Description { get; set; }
        public ResourceType Type { get; set; }
        public required int UserId { get; set; }
        public required User User { get; set; }
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
