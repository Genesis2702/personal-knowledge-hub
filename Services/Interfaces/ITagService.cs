using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITagService
    {
        public Task<Tag> AddTag(TagRequestDto tagRequest, int userId);
        public Task<List<Tag>> GetTags(int userId);
        public Task<Tag> GetTagById(int tagId, int userId);
        public Task UpdateTagById(TagRequestDto tagRequest, int tagId, int userId);
        public Task DeleteTagById(int tagId, int userId);
        public Task<Tag> RestoreTagById(int tagId, int userId);
    }
}
