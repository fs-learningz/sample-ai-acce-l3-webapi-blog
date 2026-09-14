using Blog.Domain;

namespace Blog.Application.Abstractions;

public interface IBlogPostRepository
{
    Task<IReadOnlyList<BlogPost>> ListAsync(CancellationToken cancellationToken = default);

    Task<BlogPost?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(BlogPost post, CancellationToken cancellationToken = default);

    void Remove(BlogPost post);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
