using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Infrastructure.Persistence;

public class PostRepository(BlogDbContext dbContext) : IPostRepository
{
    public IReadOnlyList<Post> GetAll() =>
        dbContext.Posts.Include(p => p.User).OrderByDescending(p => p.CreatedAt).ToList();

    public IReadOnlyList<Post> GetPublished() =>
        dbContext.Posts.Include(p => p.User)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

    public IReadOnlyList<Post> GetByAuthor(Guid userId) =>
        dbContext.Posts.Include(p => p.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

    public Post? GetById(Guid id) =>
        dbContext.Posts.Include(p => p.User).FirstOrDefault(p => p.Id == id);

    public Post Add(Post post)
    {
        dbContext.Posts.Add(post);
        dbContext.SaveChanges();
        return post;
    }

    public bool Update(Post post)
    {
        dbContext.Posts.Update(post);
        return dbContext.SaveChanges() > 0;
    }

    public bool Delete(Guid id)
    {
        var post = dbContext.Posts.Find(id);
        if (post is null)
        {
            return false;
        }

        dbContext.Posts.Remove(post);
        return dbContext.SaveChanges() > 0;
    }

    public bool AnyByAuthor(Guid userId) => dbContext.Posts.Any(p => p.UserId == userId);
}
