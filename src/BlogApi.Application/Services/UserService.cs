using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;

namespace BlogApi.Application.Services;

public class UserService(IUserRepository userRepository, IPostRepository postRepository, IPasswordHasher passwordHasher)
    : IUserService
{
    public IReadOnlyList<UserDto> GetAllUsers() =>
        userRepository.GetAll().Select(ToDto).ToList();

    public UserDto? GetUserById(Guid id) =>
        userRepository.GetById(id) is { } user ? ToDto(user) : null;

    public User? ValidateCredentials(string username, string password)
    {
        var user = userRepository.GetByUsername(username);
        return user is not null && passwordHasher.Verify(user.PasswordHash, password) ? user : null;
    }

    public (CreateUserResult Result, UserDto? User) CreateUser(CreateUserRequest request)
    {
        if (userRepository.ExistsByUsername(request.Username))
        {
            return (CreateUserResult.UsernameTaken, null);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        userRepository.Add(user);
        return (CreateUserResult.Success, ToDto(user));
    }

    public (UpdateUserResult Result, UserDto? User) UpdateUser(Guid id, UpdateUserRequest request)
    {
        var user = userRepository.GetById(id);
        if (user is null)
        {
            return (UpdateUserResult.NotFound, null);
        }

        if (!string.Equals(user.Username, request.Username, StringComparison.Ordinal) &&
            userRepository.ExistsByUsername(request.Username))
        {
            return (UpdateUserResult.UsernameTaken, null);
        }

        user.Username = request.Username;
        user.Role = request.Role;

        userRepository.Update(user);
        return (UpdateUserResult.Success, ToDto(user));
    }

    public bool ChangePassword(Guid id, string newPassword)
    {
        var user = userRepository.GetById(id);
        if (user is null)
        {
            return false;
        }

        user.PasswordHash = passwordHasher.Hash(newPassword);
        return userRepository.Update(user);
    }

    public UserDeleteResult DeleteUser(Guid id, Guid actingUserId)
    {
        if (id == actingUserId)
        {
            return UserDeleteResult.CannotDeleteSelf;
        }

        if (userRepository.GetById(id) is null)
        {
            return UserDeleteResult.NotFound;
        }

        if (postRepository.AnyByAuthor(id))
        {
            return UserDeleteResult.HasExistingPosts;
        }

        return userRepository.Delete(id) ? UserDeleteResult.Success : UserDeleteResult.NotFound;
    }

    private static UserDto ToDto(User user) => new(user.Id, user.Username, user.Role, user.CreatedAt);
}
