using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;

namespace BlogApi.Application.Services;

public class PostService(IPostRepository postRepository) : IPostService
{
    public IReadOnlyList<PostDto> GetPublishedPosts() =>
        postRepository.GetPublished().Select(ToDto).ToList();

    public IReadOnlyList<PostDto> GetAllPosts() =>
        postRepository.GetAll().Select(ToDto).ToList();

    public IReadOnlyList<PostDto> GetPostsByAuthor(Guid userId) =>
        postRepository.GetByAuthor(userId).Select(ToDto).ToList();

    public PostDto? GetPostById(Guid id) =>
        postRepository.GetById(id) is { } post ? ToDto(post) : null;

    public PostDto CreatePost(CreatePostRequest request, Guid authorId)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            UserId = authorId,
            CreatedAt = DateTime.UtcNow
        };

        postRepository.Add(post);

        return ToDto(postRepository.GetById(post.Id)!);
    }

    public PostDto? UpdatePost(Guid id, UpdatePostRequest request)
    {
        var post = postRepository.GetById(id);
        if (post is null)
        {
            return null;
        }

        post.Title = request.Title;
        post.Content = request.Content;
        post.UpdatedAt = DateTime.UtcNow;

        postRepository.Update(post);
        return ToDto(post);
    }

    public bool DeletePost(Guid id) => postRepository.Delete(id);

    public PostDto? SetPublished(Guid id, bool isPublished)
    {
        var post = postRepository.GetById(id);
        if (post is null)
        {
            return null;
        }

        post.IsPublished = isPublished;
        post.PublishedAt = isPublished ? DateTime.UtcNow : null;
        post.UpdatedAt = DateTime.UtcNow;

        postRepository.Update(post);
        return ToDto(post);
    }

    private static PostDto ToDto(Post post) =>
        new(post.Id, post.Title, post.Content, post.UserId, post.User?.Username ?? string.Empty,
            post.CreatedAt, post.UpdatedAt, post.IsPublished, post.PublishedAt);
}
