using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        public Task<bool> IsEmailExist(string email);
        public Task AddUserAsync(User user);
    }
}
