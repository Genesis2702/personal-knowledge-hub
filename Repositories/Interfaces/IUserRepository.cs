using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> IsEmailExistAsync(string email);
        public Task AddUserAsync(User user);

        public Task<User?> GetUserAsync(string email);
    }
}
