using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> IsEmailExist(string email);
        public Task AddUserAsync(User user);
    }
}
