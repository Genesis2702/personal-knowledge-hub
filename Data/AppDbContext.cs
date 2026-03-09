using Microsoft.EntityFrameworkCore;

namespace PersonalKnowledgeHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
