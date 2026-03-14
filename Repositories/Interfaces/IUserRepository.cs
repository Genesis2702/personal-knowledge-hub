using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> IsEmailExistAsync(string email);
        public Task<User> AddUserAsync(User user);
        public Task<User?> GetUserByIdAsync(int userId);
        public Task<User?> GetUserByEmailAsync(string email);
    }
}
