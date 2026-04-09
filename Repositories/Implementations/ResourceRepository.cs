using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations
{
    public class ResourceRepository : IResourceRepository
    {
        private readonly AppDbContext _dbContext;

        public ResourceRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(List<Resource>, int)> GetResourcesAsync(int userId, int pageIndex, int pageSize, int? tagId, string? search)
        {
            IQueryable<Resource> query = _dbContext.Resources.AsNoTracking().Where(resource => resource.UserId == userId);
            if (tagId.HasValue)
            {
                query = query.Where(resource => resource.ResourceTags.Any(resourceTag => resourceTag.TagId == tagId));
            }
            if (!string.IsNullOrEmpty(search))
            {

            }
            int resourcesCount = await query.CountAsync();
            List<Resource> resources = await query
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (resources, resourcesCount);
        }

        public async Task<Resource?> GetResourceByIdAsync(int resourceId)
        {
            return await _dbContext.Resources
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .SingleOrDefaultAsync(resource => resource.Id == resourceId);
        }

        public async Task<Resource> AddResourceAsync(Resource resource)
        {
            await _dbContext.Resources.AddAsync(resource);
            await _dbContext.SaveChangesAsync();
            return resource;
        }

        public async Task DeleteResourceAsync(Resource resource)
        {
            _dbContext.Resources.Remove(resource);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsTitleExistAsync(string resourceTitle, int userId)
        {
            return await _dbContext.Resources.AnyAsync(resource => resource.Title == resourceTitle && resource.UserId == userId);
        }
    }
}
