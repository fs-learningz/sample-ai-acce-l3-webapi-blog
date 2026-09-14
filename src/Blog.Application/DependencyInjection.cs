using Blog.Application.Abstractions;
using Blog.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBlogPostService, BlogPostService>();

        return services;
    }
}