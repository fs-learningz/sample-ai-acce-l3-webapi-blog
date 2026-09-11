using BlogApi.Application.Interfaces;
using BlogApi.Application.Services;
using BlogApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlogApi.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddSqliteConnection("sqlite");

        builder.Services.AddDbContext<BlogDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetSection("ConnectionStrings")["sqlite"]!));

        builder.Services.AddScoped<IPostRepository, SqlitePostRepository>();
        builder.Services.AddScoped<IPostService, PostService>();

        return builder;
    }
}
