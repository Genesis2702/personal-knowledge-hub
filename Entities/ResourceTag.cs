namespace PersonalKnowledgeHub.Entities
{
    public class ResourceTag
    {
        public Tag? Tag { get; set; }
        public int TagId { get; set; }
        public Resource? Resource { get; set; }
        public int ResourceId { get; set; }
    }
}
