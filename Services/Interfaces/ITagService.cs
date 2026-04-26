using System.Security.Claims;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITagService
    {
        public Task<Tag> AddTag(TagRequestDto tagRequest, int userId);
        public Task<List<Tag>> GetTags(int userId);
        public Task<Tag> GetTagById(int tagId, int userId);
        public Task UpdateTagById(ClaimsPrincipal user, TagRequestDto tagRequest, int tagId);
        public Task DeleteTagById(ClaimsPrincipal user, int tagId);
        public Task<Tag> RestoreTagById(ClaimsPrincipal user, int tagId);
    }
}
