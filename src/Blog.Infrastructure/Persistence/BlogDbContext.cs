using Blog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence;

public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
{
    public DbSet<BlogPost> Posts => Set<BlogPost>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);
}
