namespace BlogApi.Application.Dtos;

public record PostDto(
    Guid Id,
    string Title,
    string Content,
    string Author,
    Guid AuthorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsPublished,
    DateTime? PublishedAt);

public record CreatePostRequest(string Title, string Content);

public record UpdatePostRequest(string Title, string Content);
