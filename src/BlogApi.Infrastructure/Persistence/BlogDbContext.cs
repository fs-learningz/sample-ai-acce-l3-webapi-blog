using BlogApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Infrastructure.Persistence;

public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnType("TEXT");
            entity.Property(p => p.Title).IsRequired();
            entity.Property(p => p.Content).IsRequired();
            entity.Property(p => p.Author).IsRequired();
            entity.Property(p => p.AuthorId).HasColumnType("TEXT");
            entity.Property(p => p.CreatedAt).HasColumnType("TEXT");
            entity.Property(p => p.UpdatedAt).HasColumnType("TEXT");
            entity.Property(p => p.IsPublished).HasColumnType("INTEGER");
            entity.Property(p => p.PublishedAt).HasColumnType("TEXT");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnType("TEXT");
            entity.Property(r => r.Name).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Ignore(u => u.RoleName);
            entity.Property(u => u.Id).HasColumnType("TEXT");
            entity.Property(u => u.Username).IsRequired();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.RoleId).HasColumnType("TEXT");
            entity.Property(u => u.IsActive).HasColumnType("INTEGER");
            entity.Property(u => u.IsDefault).HasColumnType("INTEGER");
            entity.Property(u => u.CreatedAt).HasColumnType("TEXT");
            entity.Property(u => u.UpdatedAt).HasColumnType("TEXT");
        });
    }
}
