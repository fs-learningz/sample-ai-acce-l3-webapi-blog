using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize(Policy = "Admin")]
public class RolesController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<RoleDto>> GetRoles() => Ok(userService.GetRoles());
}
