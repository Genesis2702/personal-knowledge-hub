using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface ITagRepository
    {
        public Task<Tag> AddTagAsync(Tag tag);
        public Task<List<Tag>> GetTagsAsync(int userId);
        public Task UpdateTagAsync(string name, Tag tag);
        public Task DeleteTagAsync(Tag tag);
    }
}
