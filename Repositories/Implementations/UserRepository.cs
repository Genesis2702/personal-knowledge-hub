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

        public async Task AddUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
