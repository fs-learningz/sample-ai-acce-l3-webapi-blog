using BlogApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Infrastructure.Persistence;

public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();

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
            entity.Property(p => p.CreatedAt).HasColumnType("TEXT");
            entity.Property(p => p.UpdatedAt).HasColumnType("TEXT");
            entity.Property(p => p.IsPublished).HasColumnType("INTEGER");
            entity.Property(p => p.PublishedAt).HasColumnType("TEXT");
        });
    }
}
