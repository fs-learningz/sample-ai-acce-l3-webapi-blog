using BlogApi.Api.Auth;
using BlogApi.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        var username = _configuration["Admin:Username"] ?? "admin";
        var password = _configuration["Admin:Password"] ?? "admin";

        if (request.Username != username || request.Password != password)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(new LoginResponse(AdminAuthService.BearerToken, username));
    }
}
