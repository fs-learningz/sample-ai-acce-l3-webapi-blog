using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IConfiguration configuration) : ControllerBase
{
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin";

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!string.Equals(request.Username, AdminUsername, StringComparison.Ordinal)
            || !string.Equals(request.Password, AdminPassword, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var issuer = configuration["Jwt:Issuer"] ?? "emerald-blog-api";
        var audience = configuration["Jwt:Audience"] ?? "emerald-blog-web";
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            configuration.GetValue("Jwt:ExpiresMinutes", 480));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, AdminUsername),
                new Claim(ClaimTypes.Name, AdminUsername),
                new Claim(ClaimTypes.Role, "Admin")
            ],
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return Ok(new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresAtUtc));
    }
}