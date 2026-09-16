using System.ComponentModel.DataAnnotations;
using BlogApi.Application.Validation;

namespace BlogApi.Application.Dtos;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string Role,
    bool IsActive);

public record RoleDto(Guid Id, string Name);

public record CreateUserRequest(
    [Required, StringLength(32, MinimumLength = 3)] string Username,
    [Required, EmailAddress] string Email,
    [Required, PasswordComplexity] string Password,
    [Required] string Role);

public record UpdateUserRequest([Required, EmailAddress] string Email);

public record UpdateUserRoleRequest([Required] string Role);

public record ResetPasswordRequest([Required, PasswordComplexity] string NewPassword);
