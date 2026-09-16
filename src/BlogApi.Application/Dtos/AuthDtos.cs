namespace BlogApi.Application.Dtos;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt);

public record CurrentUserDto(Guid Id, string Name, string Email, string Role);
