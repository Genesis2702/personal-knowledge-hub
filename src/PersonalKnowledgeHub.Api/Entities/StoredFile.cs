namespace PersonalKnowledgeHub.Entities
{
    public class StoredFile
    {
        public int Id { get; set; }
        public required string StoredKey { get; set; }
        public required long SizeInBytes { get; set; }
        public required string ContentType { get; set; }
        public required int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public required FileFormat FileFormat { get; set; }
    }

    public enum FileFormat
    {
        Pdf,
        Png,
        Mp4
    }
}