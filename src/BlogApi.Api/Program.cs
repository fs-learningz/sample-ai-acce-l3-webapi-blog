using BlogApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.AddInfrastructure();
builder.Services.AddCors(options =>
    options.AddPolicy("WebApp", policy =>
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("WebApp");

app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
