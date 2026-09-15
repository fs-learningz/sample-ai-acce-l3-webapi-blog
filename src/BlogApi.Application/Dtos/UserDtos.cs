using BlogApi.Domain.Entities;

namespace BlogApi.Application.Dtos;

public record UserDto(Guid Id, string Username, UserRole Role, DateTime CreatedAt);

public record CreateUserRequest(string Username, string Password, UserRole Role);

public record UpdateUserRequest(string Username, UserRole Role);

public record ChangePasswordRequest(string NewPassword);

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, Guid UserId, string Username, UserRole Role);
