namespace BlogApi.Application.Dtos;

public record PostDto(
    Guid Id,
    string Title,
    string Content,
    Guid AuthorId,
    string AuthorUsername,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsPublished,
    DateTime? PublishedAt);

public record CreatePostRequest(string Title, string Content);

public record UpdatePostRequest(string Title, string Content);
