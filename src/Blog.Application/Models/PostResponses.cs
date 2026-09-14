using Blog.Domain;

namespace Blog.Application.Models;

public record PostSummaryResponse(
    int Id,
    string Title,
    string Excerpt,
    string Author,
    DateTime? PublishedAtUtc);

public record PostDetailResponse(
    int Id,
    string Title,
    string Content,
    string Author,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc);

public record AdminPostResponse(
    int Id,
    string Title,
    string Content,
    PostStatus Status,
    string Author,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc);