using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<User> AddUserAsync(User user, CancellationToken cancellationToken)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<(List<User>, int)> GetUsersAsync(int pageIndex, int pageSize, UserStatus? status, CancellationToken cancellationToken)
        {
            IQueryable<User> query = _dbContext.Users.AsNoTracking();
            if (status.HasValue)
            {
                query = query.Where(user => user.Status == status.Value);
            }
            int usersCount = await query.CountAsync(cancellationToken);
            List<User> users = await query
                .OrderBy(user => user.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .ToListAsync(cancellationToken);
            return (users, usersCount);
        }

        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<int> UpdateUserNameAsync(int userId, long version, string userName, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .Where(user => user.Id == userId && user.Version == version)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(user => user.UserName, userName)
                    .SetProperty(user => user.Version, user => user.Version + 1)
                , cancellationToken);
        }

        public async Task BanUserAsync(User user, CancellationToken cancellationToken)
        {
            user.Status = UserStatus.Banned;
            user.BannedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UnbanUserAsync(User user, CancellationToken cancellationToken)
        {
            user.Status = UserStatus.Active;
            user.BannedAt = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> ResetPasswordAsync(int userId, string newHashedPassword, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(user => user.PasswordHash, newHashedPassword)
                , cancellationToken);
        }

        public async Task<UserRole?> GetUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken)
        {
            return await _dbContext.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
        }

        public async Task<User> AddRoleToUserAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            await _dbContext.UserRoles.AddAsync(userRole, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return userRole.User;
        }

        public async Task RemoveRoleFromUserAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            _dbContext.UserRoles.Remove(userRole);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ChangeUserStatusAsync(User user, UserStatus status, CancellationToken cancellationToken)
        {
            user.Status = status;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateFailedLoginAttemptsAsync(int userId, int failedLoginLimit, int lockedMinutes, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .Where(user => user.Id == userId && (user.LockedUntil == null || user.LockedUntil < DateTime.UtcNow))
                .ExecuteUpdateAsync(update => update
                    .SetProperty(user => user.FailedLoginAttempts, user => user.FailedLoginAttempts + 1)
                    .SetProperty(user => user.LockedUntil, user => user.FailedLoginAttempts + 1 >= failedLoginLimit ? 
                        DateTime.UtcNow.AddMinutes(lockedMinutes) : user.LockedUntil), cancellationToken);
        }

        public async Task<int> ResetFailedLoginAttemptsAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(user => user.FailedLoginAttempts, 0)
                    .SetProperty(user => user.LockedUntil, (DateTime?)null)
                , cancellationToken);
        }
    }
}
