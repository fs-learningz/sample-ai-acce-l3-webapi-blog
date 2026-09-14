using Blog.Application.Abstractions;
using Blog.Infrastructure.Persistence;
using Blog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        var effectiveConnectionString = string.IsNullOrWhiteSpace(connectionString)
            ? "Data Source=blog.db"
            : connectionString;

        services.AddDbContext<BlogDbContext>(options => options.UseSqlite(effectiveConnectionString));

        services.AddScoped<IBlogPostRepository, BlogPostRepository>();

        return services;
    }
}
