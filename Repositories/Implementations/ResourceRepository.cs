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

        public async Task<(List<Resource>, int)> GetResourcesAsync(int userId, int pageIndex, int pageSize, int? tagId, ResourceType? resourceType, string? search, CancellationToken cancellationToken)
        {
            IQueryable<Resource> query = _dbContext.Resources.AsNoTracking().Where(resource => resource.UserId == userId);
            if (tagId.HasValue)
            {
                query = query.Where(resource => resource.ResourceTags.Any(resourceTag => resourceTag.TagId == tagId));
            }
            if (resourceType.HasValue)
            {
                query = query.Where(resource => resource.ResourceType == resourceType);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(resource => EF.Functions.ILike(resource.Title, $"%{search}%")
                || (resource.Description != null && EF.Functions.ILike(resource.Description, $"%{search}%")));
            }
            int resourcesCount = await query.CountAsync(cancellationToken);
            List<Resource> resources = await query
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (resources, resourcesCount);
        }

        public async Task<Resource?> GetResourceByIdAsync(int resourceId, CancellationToken cancellationToken)
        {
            return await _dbContext.Resources
                .Include(resource => resource.ResourceTags)
                .ThenInclude(resourceTag => resourceTag.Tag)
                .SingleOrDefaultAsync(resource => resource.Id == resourceId, cancellationToken);
        }

        public async Task<Resource?> GetResourceByIdForRestoreAsync(int resourceId, CancellationToken cancellationToken)
        {
            return await _dbContext.Resources.IgnoreQueryFilters()
                .SingleOrDefaultAsync(resource => resource.Id == resourceId, cancellationToken);
        }

        public async Task<Resource> AddResourceAsync(Resource resource, CancellationToken cancellationToken)
        {
            await _dbContext.Resources.AddAsync(resource, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return resource;
        }

        public async Task<int> UpdateResourceAsync(int resourceId, long version, string? title, string? url, string? description, CancellationToken cancellationToken)
        {
            return await _dbContext.Resources
                .Where(resource => resource.Id == resourceId && resource.Version == version)
                .ExecuteUpdateAsync(update =>
                    update
                        .SetProperty(resource => resource.Title, resource => title != null ? title : resource.Title)
                        .SetProperty(resource => resource.Url, resource => url != null ? url : resource.Url)
                        .SetProperty(resource => resource.Description,
                            resource => description != null ? description : resource.Description)
                        .SetProperty(resource => resource.LastModified, resource => DateTime.UtcNow)
                        .SetProperty(resource => resource.Version, resource => resource.Version + 1)
                , cancellationToken);
        }

        public async Task DeleteResourceAsync(Resource resource, int userId, CancellationToken cancellationToken)
        {
            resource.IsDeleted = true;
            resource.DeletedAt = DateTime.UtcNow;
            resource.DeletedBy = userId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Resource> RestoreResourceAsync(Resource resource, CancellationToken cancellationToken)
        {
            resource.IsDeleted = false;
            resource.DeletedAt = null;
            resource.DeletedBy = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return resource;
        }

        public async Task CleanUpResourcesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Resources.Where(resource => resource.IsDeleted).ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<bool> IsTitleExistAsync(string resourceTitle, int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Resources.AnyAsync(resource => resource.Title == resourceTitle && resource.UserId == userId, cancellationToken);
        }
    }
}
