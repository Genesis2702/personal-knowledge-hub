using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> IsEmailExistAsync(string email);
        public Task<User> AddUserAsync(User user);
        public Task<User?> GetUserByIdAsync(int userId);
        public Task<User?> GetUserByEmailAsync(string email);
        public Task BanUserAsync(User user);
        public Task UnbanUserAsync(User user);
        public Task ResetPasswordAsync(User user, string newHashedPassword);
    }
}
