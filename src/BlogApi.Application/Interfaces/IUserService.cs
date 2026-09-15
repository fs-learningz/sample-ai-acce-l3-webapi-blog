using BlogApi.Application.Dtos;
using BlogApi.Domain.Entities;

namespace BlogApi.Application.Interfaces;

public enum UserDeleteResult
{
    Success,
    NotFound,
    CannotDeleteSelf,
    HasExistingPosts
}

public enum CreateUserResult
{
    Success,
    UsernameTaken
}

public enum UpdateUserResult
{
    Success,
    NotFound,
    UsernameTaken
}

public interface IUserService
{
    IReadOnlyList<UserDto> GetAllUsers();
    UserDto? GetUserById(Guid id);
    User? ValidateCredentials(string username, string password);
    (CreateUserResult Result, UserDto? User) CreateUser(CreateUserRequest request);
    (UpdateUserResult Result, UserDto? User) UpdateUser(Guid id, UpdateUserRequest request);
    bool ChangePassword(Guid id, string newPassword);
    UserDeleteResult DeleteUser(Guid id, Guid actingUserId);
}
