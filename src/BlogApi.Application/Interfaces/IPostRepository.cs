using BlogApi.Domain.Entities;

namespace BlogApi.Application.Interfaces;

public interface IPostRepository
{
    IReadOnlyList<Post> GetAll();
    IReadOnlyList<Post> GetPublished();
    IReadOnlyList<Post> GetByAuthor(Guid authorId);
    Post? GetById(Guid id);
    Post Add(Post post);
    bool Update(Post post);
    bool Delete(Guid id);
}
