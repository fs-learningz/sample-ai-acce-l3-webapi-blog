using System.Text.RegularExpressions;
using Blog.Application.Abstractions;
using Blog.Application.Models;
using Blog.Domain;

namespace Blog.Application.Services;

public partial class BlogPostService(IBlogPostRepository repository) : IBlogPostService
{
    public async Task<IReadOnlyList<PostSummaryResponse>> GetPublishedPostsAsync(CancellationToken cancellationToken = default)
    {
        var posts = await repository.ListAsync(cancellationToken);

        return posts
            .Where(post => post.Status == PostStatus.Published)
            .OrderByDescending(post => post.PublishedAtUtc)
            .Select(post => ToSummary(post))
            .ToList();
    }

    public async Task<PostDetailResponse?> GetPublishedPostAsync(int id, CancellationToken cancellationToken = default)
    {
        var post = await repository.GetByIdAsync(id, cancellationToken);

        if (post is null || post.Status != PostStatus.Published)
        {
            return null;
        }

        return ToDetail(post);
    }

    public async Task<IReadOnlyList<AdminPostResponse>> GetAllPostsAsync(CancellationToken cancellationToken = default)
    {
        var posts = await repository.ListAsync(cancellationToken);

        return posts
            .OrderByDescending(post => post.UpdatedAtUtc)
            .Select(post => ToAdminResponse(post))
            .ToList();
    }

    public async Task<AdminPostResponse?> GetPostByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var post = await repository.GetByIdAsync(id, cancellationToken);
        return post is null ? null : ToAdminResponse(post);
    }

    public async Task<AdminPostResponse> CreatePostAsync(CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var post = new BlogPost
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Status = request.Status,
            Author = "admin",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            PublishedAtUtc = request.Status == PostStatus.Published ? now : null
        };

        await repository.AddAsync(post, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ToAdminResponse(post);
    }

    public async Task<AdminPostResponse?> UpdatePostAsync(int id, UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await repository.GetByIdAsync(id, cancellationToken);

        if (post is null)
        {
            return null;
        }

        post.Title = request.Title.Trim();
        post.Content = request.Content.Trim();
        post.Status = request.Status;
        post.UpdatedAtUtc = DateTime.UtcNow;

        if (post.Status == PostStatus.Published && post.PublishedAtUtc is null)
        {
            post.PublishedAtUtc = post.UpdatedAtUtc;
        }

        await repository.SaveChangesAsync(cancellationToken);

        return ToAdminResponse(post);
    }

    public async Task<bool> DeletePostAsync(int id, CancellationToken cancellationToken = default)
    {
        var post = await repository.GetByIdAsync(id, cancellationToken);

        if (post is null)
        {
            return false;
        }

        repository.Remove(post);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static PostSummaryResponse ToSummary(BlogPost post) =>
        new(post.Id, post.Title, CreateExcerpt(post.Content), post.Author, post.PublishedAtUtc);

    private static PostDetailResponse ToDetail(BlogPost post) =>
        new(post.Id, post.Title, post.Content, post.Author, post.CreatedAtUtc, post.UpdatedAtUtc, post.PublishedAtUtc);

    private static AdminPostResponse ToAdminResponse(BlogPost post) =>
        new(post.Id, post.Title, post.Content, post.Status, post.Author, post.CreatedAtUtc, post.UpdatedAtUtc, post.PublishedAtUtc);

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    private static string CreateExcerpt(string content)
    {
        var normalized = WhitespaceRegex().Replace(content, " ").Trim();
        return normalized.Length <= 180 ? normalized : $"{normalized[..177]}...";
    }
}