using System.Security.Claims;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITagService
    {
        public Task<Tag> AddTag(TagRequestDto tagRequest, int userId, CancellationToken cancellationToken);
        public Task<List<Tag>> GetTags(int userId, CancellationToken cancellationToken);
        public Task<Tag> GetTagById(int tagId, int userId, CancellationToken cancellationToken);
        public Task UpdateTagById(ClaimsPrincipal user, TagRequestDto tagRequest, int tagId, CancellationToken cancellationToken);
        public Task DeleteTagById(ClaimsPrincipal user, int tagId, CancellationToken cancellationToken);
        public Task<Tag> RestoreTagById(ClaimsPrincipal user, int tagId, CancellationToken cancellationToken);
    }
}
