using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface ITagRepository
    {
        public Task<Tag> AddTagAsync(Tag tag, CancellationToken cancellationToken);
        public Task<List<Tag>> GetTagsAsync(int userId, CancellationToken cancellationToken);
        public Task<Tag?> GetTagByIdAsync(int tagId, CancellationToken cancellationToken);
        public Task<Tag?> GetTagByIdForRestoreAsync(int tagId, CancellationToken cancellationToken);
        public Task<int> UpdateTagAsync(int tagId, long version, string tagName, CancellationToken cancellationToken);
        public Task DeleteTagAsync(Tag tag, int userId, CancellationToken cancellationToken);
        public Task<Tag> RestoreTagAsync(Tag tag, CancellationToken cancellationToken);
        public Task<bool> IsTagExistAsync(string tagName, int userId, CancellationToken cancellationToken);
    }
}
