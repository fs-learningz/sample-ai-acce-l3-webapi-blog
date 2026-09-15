using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = "Admin")]
public class AdminUsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<UserDto>> GetUsers() => Ok(userService.GetAllUsers());

    [HttpPost]
    public ActionResult<UserDto> CreateUser(CreateUserRequest request) =>
        ToActionResult(userService.CreateUser(request), created: true);

    [HttpPut("{id:guid}")]
    public ActionResult<UserDto> UpdateUser(Guid id, UpdateUserRequest request) =>
        ToActionResult(userService.UpdateUser(id, request));

    [HttpPut("{id:guid}/role")]
    public ActionResult<UserDto> UpdateUserRole(Guid id, UpdateUserRoleRequest request) =>
        ToActionResult(userService.UpdateUserRole(id, request));

    [HttpPost("{id:guid}/deactivate")]
    public ActionResult<UserDto> Deactivate(Guid id) =>
        ToActionResult(userService.SetActive(id, false));

    [HttpPost("{id:guid}/activate")]
    public ActionResult<UserDto> Activate(Guid id) =>
        ToActionResult(userService.SetActive(id, true));

    [HttpPost("{id:guid}/reset-password")]
    public ActionResult<UserDto> ResetPassword(Guid id, ResetPasswordRequest request) =>
        ToActionResult(userService.ResetPassword(id, request));

    private ActionResult<UserDto> ToActionResult(UserServiceResult<UserDto> result, bool created = false)
    {
        if (result.IsSuccess)
        {
            return created ? StatusCode(StatusCodes.Status201Created, result.Value) : Ok(result.Value);
        }

        return result.Error switch
        {
            UserServiceError.NotFound => NotFound(new { message = result.ErrorMessage }),
            UserServiceError.DuplicateUsername => Conflict(new { message = result.ErrorMessage }),
            UserServiceError.InvalidRole => BadRequest(new { message = result.ErrorMessage }),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
    }
}
