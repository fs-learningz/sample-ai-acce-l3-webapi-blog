using BlogApi.Api.Auth;
using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<UserDto>> GetUsers() => Ok(userService.GetAllUsers());

    [HttpGet("{id:guid}")]
    public ActionResult<UserDto> GetUser(Guid id)
    {
        var user = userService.GetUserById(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public ActionResult<UserDto> CreateUser(CreateUserRequest request)
    {
        var (result, user) = userService.CreateUser(request);
        return result switch
        {
            CreateUserResult.UsernameTaken => Conflict(new { message = "Username is already taken." }),
            _ => CreatedAtAction(nameof(GetUser), new { id = user!.Id }, user)
        };
    }

    [HttpPut("{id:guid}")]
    public ActionResult<UserDto> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var (result, user) = userService.UpdateUser(id, request);
        return result switch
        {
            UpdateUserResult.NotFound => NotFound(),
            UpdateUserResult.UsernameTaken => Conflict(new { message = "Username is already taken." }),
            _ => Ok(user)
        };
    }

    [HttpPut("{id:guid}/password")]
    public IActionResult ChangePassword(Guid id, ChangePasswordRequest request) =>
        userService.ChangePassword(id, request.NewPassword) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteUser(Guid id)
    {
        var result = userService.DeleteUser(id, User.GetUserId());
        return result switch
        {
            UserDeleteResult.Success => NoContent(),
            UserDeleteResult.NotFound => NotFound(),
            UserDeleteResult.CannotDeleteSelf => BadRequest(new { message = "You cannot delete your own account." }),
            UserDeleteResult.HasExistingPosts => BadRequest(new { message = "Cannot delete a user who still owns posts. Reassign or delete their posts first." }),
            _ => BadRequest()
        };
    }
}
