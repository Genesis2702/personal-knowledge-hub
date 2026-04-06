namespace PersonalKnowledgeHub.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<ResourceTag> ResourceTags { get; set; } = new List<ResourceTag>();
    }
}
