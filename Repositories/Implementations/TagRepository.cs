using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _dbContext;

        public TagRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Tag> AddTagAsync(Tag tag, CancellationToken cancellationToken)
        {
            await _dbContext.Tags.AddAsync(tag, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return tag;
        }

        public async Task<List<Tag>> GetTagsAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Tags.AsNoTracking().Where(tag => tag.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<Tag?> GetTagByIdAsync(int tagId, CancellationToken cancellationToken)
        {
            return await _dbContext.Tags.SingleOrDefaultAsync(tag => tag.Id == tagId, cancellationToken);
        }

        public async Task<Tag?> GetTagByIdForRestoreAsync(int tagId, CancellationToken cancellationToken)
        {
            return await _dbContext.Tags.IgnoreQueryFilters().SingleOrDefaultAsync(tag => tag.Id == tagId, cancellationToken);
        }

        public async Task<int> UpdateTagAsync(int tagId, long version, string tagName, CancellationToken cancellationToken)
        {
            return await _dbContext.Tags
                .Where(tag => tag.Id == tagId && tag.Version == version)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(tag => tag.Name, tagName)
                    .SetProperty(tag => tag.LastModified, DateTime.UtcNow)
                    .SetProperty(tag => tag.Version, tag => tag.Version + 1)
                , cancellationToken);
        }

        public async Task DeleteTagAsync(Tag tag, int userId, CancellationToken cancellationToken)
        {
            tag.IsDeleted = true;
            tag.DeletedAt = DateTime.UtcNow;
            tag.DeletedBy = userId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Tag> RestoreTagAsync(Tag tag, CancellationToken cancellationToken)
        {
            tag.IsDeleted = false;
            tag.DeletedAt = null;
            tag.DeletedBy = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return tag;
        }

        public async Task<bool> IsTagExistAsync(string tagName, int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Tags.AnyAsync(tag => tag.Name == tagName && tag.UserId == userId, cancellationToken);
        }
    }
}
