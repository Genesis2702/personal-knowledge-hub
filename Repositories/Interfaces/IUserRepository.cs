using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> IsEmailExistAsync(string email);
        public Task<User> AddUserAsync(User user);
        public Task<(List<User>, int)> GetUsersAsync(int pageIndex, int pageSize, UserStatus? status);
        public Task<User?> GetUserByIdAsync(int userId);
        public Task<User?> GetUserByEmailAsync(string email);
        public Task BanUserAsync(User user);
        public Task UnbanUserAsync(User user);
        public Task ResetPasswordAsync(User user, string newHashedPassword);
        public Task<UserRole?> GetUserRoleAsync(int userId, int roleId);
        public Task<User> AddRoleToUserAsync(UserRole userRole);
        public Task RemoveRoleFromUserAsync(UserRole userRole);
    }
}
