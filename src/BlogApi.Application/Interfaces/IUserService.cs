using BlogApi.Application.Dtos;

namespace BlogApi.Application.Interfaces;

public interface IUserService
{
    IReadOnlyList<UserDto> GetAllUsers();
    IReadOnlyList<RoleDto> GetRoles();
    UserServiceResult<UserDto> CreateUser(CreateUserRequest request);
    UserServiceResult<UserDto> UpdateUser(Guid id, UpdateUserRequest request);
    UserServiceResult<UserDto> UpdateUserRole(Guid id, UpdateUserRoleRequest request);
    UserServiceResult<UserDto> SetActive(Guid id, bool isActive);
    UserServiceResult<UserDto> ResetPassword(Guid id, ResetPasswordRequest request);
}

public enum UserServiceError
{
    None,
    NotFound,
    DuplicateUsername,
    InvalidRole
}

public record UserServiceResult<T>(T? Value, UserServiceError Error, string? ErrorMessage)
{
    public bool IsSuccess => Error == UserServiceError.None;

    public static UserServiceResult<T> Success(T value) => new(value, UserServiceError.None, null);

    public static UserServiceResult<T> Failure(UserServiceError error, string message) =>
        new(default, error, message);
}
