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

        public async Task<Tag> AddTagAsync(Tag tag)
        {
            await _dbContext.Tags.AddAsync(tag);
            await _dbContext.SaveChangesAsync();
            return tag;
        }

        public async Task<List<Tag>> GetTagsAsync(int userId)
        {
            return await _dbContext.Tags.AsNoTracking().Where(tag => tag.UserId == userId).ToListAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            return await _dbContext.Tags.SingleOrDefaultAsync(tag => tag.Id == tagId);
        }

        public async Task<Tag?> GetTagByIdForRestoreAsync(int tagId)
        {
            return await _dbContext.Tags.IgnoreQueryFilters().SingleOrDefaultAsync(tag => tag.Id == tagId);
        }

        public async Task UpdateTagAsync(string tagName, Tag tag)
        {
            tag.Name = tagName;
            tag.LastModified = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(Tag tag, int userId)
        {
            tag.IsDeleted = true;
            tag.DeletedAt = DateTime.UtcNow;
            tag.DeletedBy = userId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Tag> RestoreTagAsync(Tag tag)
        {
            tag.IsDeleted = false;
            tag.DeletedAt = null;
            tag.DeletedBy = null;
            await _dbContext.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> IsTagExistAsync(string tagName, int userId)
        {
            return await _dbContext.Tags.AnyAsync(tag => tag.Name == tagName && tag.UserId == userId);
        }
    }
}
