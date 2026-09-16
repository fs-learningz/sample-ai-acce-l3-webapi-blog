using BlogApi.Api.Auth;
using BlogApi.Application.Dtos;
using BlogApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IUserRepository userRepository, IJwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        var user = userRepository.GetByUsername(request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Your account is inactive. Contact an administrator." });
        }

        var (token, expiresAt) = tokenService.CreateToken(user);
        return Ok(new LoginResponse(token, expiresAt));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<CurrentUserDto> Me()
    {
        var sub = User.FindFirst(JwtClaimTypes.Subject)?.Value;
        var name = User.FindFirst(JwtClaimTypes.Name)?.Value;
        var email = User.FindFirst(JwtClaimTypes.Email)?.Value;
        var role = User.FindFirst(JwtClaimTypes.Role)?.Value;

        if (sub is null || name is null || email is null || role is null)
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserDto(Guid.Parse(sub), name, email, role));
    }
}
