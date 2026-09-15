using System.Text;
using BlogApi.Api.Auth;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;
using BlogApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.AddInfrastructure();
builder.Services.AddCors(options =>
    options.AddPolicy("WebApp", policy =>
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

var signingKey = jwtSection["SigningKey"];
if (string.IsNullOrEmpty(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
{
    throw new InvalidOperationException(
        "Configuration value 'Jwt:SigningKey' (env: Jwt__SigningKey) is missing or shorter than 32 bytes.");
}

var jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Configuration value 'Jwt:Issuer' is missing.");
var jwtAudience = jwtSection["Audience"] ?? throw new InvalidOperationException("Configuration value 'Jwt:Audience' is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            NameClaimType = JwtClaimTypes.Subject,
            RoleClaimType = JwtClaimTypes.Role,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var subject = context.Principal?.FindFirst(JwtClaimTypes.Subject)?.Value;
                if (subject is null || !Guid.TryParse(subject, out var userId))
                {
                    context.Fail("Invalid token subject.");
                    return Task.CompletedTask;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var user = userRepository.GetById(userId);
                if (user is null || !user.IsActive)
                {
                    context.Fail("User no longer exists or is inactive.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole(RoleNames.Admin))
    .AddPolicy("Author", policy => policy.RequireRole(RoleNames.Author, RoleNames.Admin));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("WebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
