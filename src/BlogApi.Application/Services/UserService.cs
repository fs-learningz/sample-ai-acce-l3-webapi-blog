using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;

namespace BlogApi.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public IReadOnlyList<UserDto> GetAllUsers() =>
        userRepository.GetAll().Where(u => !u.IsDefault).Select(ToDto).ToList();

    public IReadOnlyList<RoleDto> GetRoles() =>
        userRepository.GetRoles().Select(r => new RoleDto(r.Id, r.Name)).ToList();

    public UserServiceResult<UserDto> CreateUser(CreateUserRequest request)
    {
        if (userRepository.GetByUsername(request.Username) is not null)
        {
            return UserServiceResult<UserDto>.Failure(
                UserServiceError.DuplicateUsername,
                $"Username '{request.Username}' is already taken.");
        }

        var role = userRepository.GetRoleByName(request.Role);
        if (role is null)
        {
            return UserServiceResult<UserDto>.Failure(
                UserServiceError.InvalidRole,
                $"Role '{request.Role}' does not exist.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.Id,
            RoleName = role.Name,
            IsActive = false,
            IsDefault = false,
            CreatedAt = DateTime.UtcNow
        };

        userRepository.Add(user);
        return UserServiceResult<UserDto>.Success(ToDto(user));
    }

    public UserServiceResult<UserDto> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = FindManageableUser(id);
        if (user is null)
        {
            return NotFound<UserDto>();
        }

        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        userRepository.Update(user);
        return UserServiceResult<UserDto>.Success(ToDto(user));
    }

    public UserServiceResult<UserDto> UpdateUserRole(Guid id, UpdateUserRoleRequest request)
    {
        var user = FindManageableUser(id);
        if (user is null)
        {
            return NotFound<UserDto>();
        }

        var role = userRepository.GetRoleByName(request.Role);
        if (role is null)
        {
            return UserServiceResult<UserDto>.Failure(
                UserServiceError.InvalidRole,
                $"Role '{request.Role}' does not exist.");
        }

        user.RoleId = role.Id;
        user.RoleName = role.Name;
        user.UpdatedAt = DateTime.UtcNow;

        userRepository.Update(user);
        return UserServiceResult<UserDto>.Success(ToDto(user));
    }

    public UserServiceResult<UserDto> SetActive(Guid id, bool isActive)
    {
        var user = FindManageableUser(id);
        if (user is null)
        {
            return NotFound<UserDto>();
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        userRepository.Update(user);
        return UserServiceResult<UserDto>.Success(ToDto(user));
    }

    public UserServiceResult<UserDto> ResetPassword(Guid id, ResetPasswordRequest request)
    {
        var user = FindManageableUser(id);
        if (user is null)
        {
            return NotFound<UserDto>();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        userRepository.Update(user);
        return UserServiceResult<UserDto>.Success(ToDto(user));
    }

    private User? FindManageableUser(Guid id)
    {
        var user = userRepository.GetById(id);
        return user is null || user.IsDefault ? null : user;
    }

    private static UserServiceResult<T> NotFound<T>() =>
        UserServiceResult<T>.Failure(UserServiceError.NotFound, "User not found.");

    private static UserDto ToDto(User user) =>
        new(user.Id, user.Username, user.Email, user.RoleName, user.IsActive);
}
