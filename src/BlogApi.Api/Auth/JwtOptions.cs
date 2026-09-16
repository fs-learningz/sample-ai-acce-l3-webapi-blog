namespace BlogApi.Api.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string SigningKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int LifetimeMinutes { get; set; } = 60;
}
