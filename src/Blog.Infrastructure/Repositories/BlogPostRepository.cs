using Blog.Application.Abstractions;
using Blog.Domain;
using Blog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class BlogPostRepository(BlogDbContext context) : IBlogPostRepository
{
    public async Task<IReadOnlyList<BlogPost>> ListAsync(CancellationToken cancellationToken = default)
        => await context.Posts.AsNoTracking().ToListAsync(cancellationToken);

    public Task<BlogPost?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Posts.FirstOrDefaultAsync(post => post.Id == id, cancellationToken);

    public async Task AddAsync(BlogPost post, CancellationToken cancellationToken = default)
        => await context.Posts.AddAsync(post, cancellationToken);

    public void Remove(BlogPost post) => context.Posts.Remove(post);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
