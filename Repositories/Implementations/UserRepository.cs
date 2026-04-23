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

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.SingleOrDefaultAsync(user => user.Email == email);
        }

        public async Task BanUserAsync(User user)
        {
            user.IsBanned = true;
            user.BannedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnbanUserAsync(User user)
        {
            user.IsBanned = false;
            user.BannedAt = null;
            await _dbContext.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(User user, string newHashedPassword)
        {
            user.PasswordHash = newHashedPassword;
            await _dbContext.SaveChangesAsync();
        }
    }
}
