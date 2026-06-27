using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken);
        public Task<User> AddUserAsync(User user, CancellationToken cancellationToken);
        public Task<(List<User>, int)> GetUsersAsync(int pageIndex, int pageSize, UserStatus? status, CancellationToken cancellationToken);
        public Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken);
        public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
        public Task<int> UpdateUserNameAsync(int userId, long version, string userName, CancellationToken cancellationToken);
        public Task BanUserAsync(User user, CancellationToken cancellationToken);
        public Task UnbanUserAsync(User user, CancellationToken cancellationToken);
        public Task<int> ResetPasswordAsync(int userId, string newHashedPassword, CancellationToken cancellationToken);
        public Task<UserRole?> GetUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken);
        public Task<User> AddRoleToUserAsync(UserRole userRole, CancellationToken cancellationToken);
        public Task RemoveRoleFromUserAsync(UserRole userRole, CancellationToken cancellationToken);
        public Task ChangeUserStatusAsync(User user, UserStatus status, CancellationToken cancellationToken);
        public Task<int> UpdateFailedLoginAttemptsAsync(int userId, int failedLoginLimit, int lockedMinutes, CancellationToken cancellationToken);
        public Task<int> ResetFailedLoginAttemptsAsync(int userId, CancellationToken cancellationToken);
    }
}
