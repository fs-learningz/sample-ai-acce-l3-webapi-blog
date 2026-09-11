namespace BlogApi.Application.Dtos;

public record PostDto(
    Guid Id,
    string Title,
    string Content,
    string Author,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsPublished,
    DateTime? PublishedAt);

public record CreatePostRequest(string Title, string Content, string Author);

public record UpdatePostRequest(string Title, string Content);

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string Username);
