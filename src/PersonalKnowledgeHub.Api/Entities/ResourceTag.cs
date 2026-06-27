namespace PersonalKnowledgeHub.Entities
{
    public class ResourceTag
    {
        public required Tag Tag { get; set; }
        public int TagId { get; set; }
        public required Resource Resource { get; set; }
        public int ResourceId { get; set; }
    }
}
