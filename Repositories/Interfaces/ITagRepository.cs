using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface ITagRepository
    {
        public Task<Tag> AddTagAsync(Tag tag);
        public Task<List<Tag>> GetTagsAsync(int userId);
        public Task<Tag?> GetTagByIdAsync(int tagId);
        public Task<Tag?> GetTagByIdForRestoreAsync(int tagId);
        public Task UpdateTagAsync(string tagName, Tag tag);
        public Task DeleteTagAsync(Tag tag, int userId);
        public Task<Tag> RestoreTagAsync(Tag tag);
        public Task<bool> IsTagExistAsync(string tagName, int userId);
    }
}
