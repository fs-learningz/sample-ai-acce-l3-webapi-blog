using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Models;

public class LoginRequest
{
    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record LoginResponse(
    string Token,
    string TokenType,
    DateTime ExpiresAtUtc);