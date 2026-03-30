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
            return await _dbContext.Tags.Where(tag => tag.UserId == userId).ToListAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            return await _dbContext.Tags.SingleOrDefaultAsync(tag => tag.Id == tagId);
        }

        public async Task UpdateTagAsync(string tagName, Tag tag)
        {
            tag.Name = tagName;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(Tag tag)
        {
            _dbContext.Tags.Remove(tag);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsTagExistAsync(string tagName, int userId)
        {
            return await _dbContext.Tags.AnyAsync(tag => tag.Name == tagName && tag.UserId == userId);
        }
    }
}
