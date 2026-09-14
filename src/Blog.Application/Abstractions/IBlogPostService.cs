using Blog.Application.Models;

namespace Blog.Application.Abstractions;

public interface IBlogPostService
{
    Task<IReadOnlyList<PostSummaryResponse>> GetPublishedPostsAsync(CancellationToken cancellationToken = default);

    Task<PostDetailResponse?> GetPublishedPostAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminPostResponse>> GetAllPostsAsync(CancellationToken cancellationToken = default);

    Task<AdminPostResponse?> GetPostByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<AdminPostResponse> CreatePostAsync(CreatePostRequest request, CancellationToken cancellationToken = default);

    Task<AdminPostResponse?> UpdatePostAsync(int id, UpdatePostRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeletePostAsync(int id, CancellationToken cancellationToken = default);
}