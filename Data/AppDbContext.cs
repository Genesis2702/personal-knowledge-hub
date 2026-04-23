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
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ResourceTag> ResourceTags { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

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
            modelBuilder.Entity<Resource>().HasQueryFilter(resource => !resource.IsDeleted);

            modelBuilder.Entity<Tag>().HasKey(tag => tag.Id);
            modelBuilder.Entity<Tag>()
                .HasOne(tag => tag.User)
                .WithMany(user => user.Tags)
                .HasForeignKey(tag => tag.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Tag>()
                .HasIndex(tag => new { tag.UserId, tag.Name })
                .IsUnique();

            modelBuilder.Entity<ResourceTag>().HasKey(resourceTag => new { resourceTag.TagId, resourceTag.ResourceId });
            modelBuilder.Entity<ResourceTag>()
                .HasOne(resourceTag => resourceTag.Tag)
                .WithMany(tag => tag.ResourceTags)
                .HasForeignKey(resourceTag => resourceTag.TagId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ResourceTag>()
                .HasOne(resourceTag => resourceTag.Resource)
                .WithMany(resource => resource.ResourceTags)
                .HasForeignKey(resourceTag => resourceTag.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>().HasKey(role => role.Id);
            modelBuilder.Entity<Role>().HasIndex(role => role.Name).IsUnique();
            
            modelBuilder.Entity<Permission>().HasKey(permission => permission.Id);
            modelBuilder.Entity<Permission>().HasIndex(permission => permission.Name).IsUnique();
            
            modelBuilder.Entity<UserRole>().HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<RolePermission>().HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
            modelBuilder.Entity<RolePermission>()
                .HasOne(rolePermission => rolePermission.Role)
                .WithMany(role => role.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RolePermission>()
                .HasOne(rolePermission => rolePermission.Permission)
                .WithMany(permission => permission.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
