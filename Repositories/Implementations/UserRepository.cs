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

        public async Task<bool> IsEmailExistAsync(string email)
        {
            return await _dbContext.Users.AnyAsync(user => user.Email == email);
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<(List<User>, int)> GetUsersAsync(int pageIndex, int pageSize, UserStatus? status)
        {
            IQueryable<User> query = _dbContext.Users.AsNoTracking();
            if (status.HasValue)
            {
                query = query.Where(user => user.Status == status.Value);
            }
            int usersCount = await query.CountAsync();
            List<User> users = await query
                .OrderBy(user => user.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .ToListAsync();
            return (users, usersCount);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _dbContext.Users
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .SingleOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.SingleOrDefaultAsync(user => user.Email == email);
        }

        public async Task BanUserAsync(User user)
        {
            user.Status = UserStatus.Banned;
            user.BannedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnbanUserAsync(User user)
        {
            user.Status = UserStatus.Active;
            user.BannedAt = null;
            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(User user, string newHashedPassword)
        {
            user.PasswordHash = newHashedPassword;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserRole?> GetUserRoleAsync(int userId, int roleId)
        {
            return await _dbContext.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<User> AddRoleToUserAsync(UserRole userRole)
        {
            await _dbContext.UserRoles.AddAsync(userRole);
            await _dbContext.SaveChangesAsync();
            return userRole.User;
        }

        public async Task RemoveRoleFromUserAsync(UserRole userRole)
        {
            _dbContext.UserRoles.Remove(userRole);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ChangeUserStatusAsync(User user, UserStatus status)
        {
            user.Status = status;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateFailedLoginAttemptsAsync(int userId, int failedLoginLimit, int lockedMinutes)
        {
            return await _dbContext.Users
                .Where(user => user.Id == userId && (user.LockedUntil == null || user.LockedUntil < DateTime.UtcNow))
                .ExecuteUpdateAsync(update => update
                    .SetProperty(user => user.FailedLoginAttempts, user => user.FailedLoginAttempts + 1)
                    .SetProperty(user => user.LockedUntil, user => user.FailedLoginAttempts + 1 >= failedLoginLimit ? 
                        DateTime.UtcNow.AddMinutes(lockedMinutes) : user.LockedUntil));
        }

        public async Task<int> ResetFailedLoginAttemptsAsync(int userId)
        {
            return await _dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(user => user.FailedLoginAttempts, 0)
                    .SetProperty(user => user.LockedUntil, (DateTime?)null));
        }
    }
}
