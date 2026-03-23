using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Resource> Resources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>().HasKey(refreshToken => refreshToken.Id);
            modelBuilder.Entity<RefreshToken>()
                .HasOne(refreshToken => refreshToken.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(refreshToken => refreshToken.Token)
                .IsUnique();

            modelBuilder.Entity<Resource>().HasKey(resource => resource.Id);
            modelBuilder.Entity<Resource>()
                .HasOne(resource => resource.User)
                .WithMany(user => user.Resources)
                .HasForeignKey(resource => resource.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Resource>()
                .Property(resource => resource.ResourceType)
                .HasConversion<string>();
            modelBuilder.Entity<Resource>()
                .HasIndex(resource => new { resource.UserId, resource.Title })
                .IsUnique();
        }
    }
}
