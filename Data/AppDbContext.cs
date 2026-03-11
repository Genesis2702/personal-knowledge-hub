using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
