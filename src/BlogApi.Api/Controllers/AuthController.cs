using BlogApi.Api.Auth;
using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        var user = userService.ValidateCredentials(request.Username, request.Password);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var token = tokenService.GenerateToken(user);
        return Ok(new LoginResponse(token, user.Id, user.Username, user.Role));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        var user = userService.GetUserById(User.GetUserId());
        return user is null ? Unauthorized() : Ok(user);
    }
}
