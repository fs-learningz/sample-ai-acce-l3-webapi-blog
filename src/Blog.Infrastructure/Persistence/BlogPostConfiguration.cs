using Blog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    private static readonly DateTime SeedPublishedAt1 = new(2026, 8, 20, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SeedPublishedAt2 = new(2026, 8, 26, 14, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime SeedPublishedAt3 = new(2026, 9, 2, 11, 15, 0, DateTimeKind.Utc);
    private static readonly DateTime SeedDraftAt = new(2026, 9, 12, 16, 45, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.Property(post => post.Title).HasMaxLength(200).IsRequired();
        builder.Property(post => post.Content).IsRequired();
        builder.Property(post => post.Author).HasMaxLength(100);
        builder.Property(post => post.Status).HasConversion<int>();

        builder.HasIndex(post => post.Status);

        builder.HasData(
            new BlogPost
            {
                Id = 1,
                Title = "Welcome to the Emerald Blog",
                Content = """
                    Welcome to our little corner of the internet! This blog is built as a full-stack sample: an ASP.NET Core Web API following Clean Architecture on the back end, and an Angular application dressed up with daisyUI's emerald theme on the front end.

                    Behind the scenes, everything is orchestrated with .NET Aspire. The AppHost wires up the API, a SQLite database, and a sqlite-web instance so you can peek at the data any time you like.

                    Browse around, read some posts, and if you are the admin, log in and start writing!
                    """,
                Status = PostStatus.Published,
                Author = "admin",
                CreatedAtUtc = SeedPublishedAt1,
                UpdatedAtUtc = SeedPublishedAt1,
                PublishedAtUtc = SeedPublishedAt1
            },
            new BlogPost
            {
                Id = 2,
                Title = "Why Clean Architecture still matters",
                Content = """
                    Clean Architecture is all about keeping business logic at the center of your solution, with infrastructure and delivery mechanisms pushed to the edges.

                    In this solution, Blog.Domain holds the entities and nothing else. Blog.Application depends only on the domain and defines the use cases and abstractions. Blog.Infrastructure implements those abstractions with EF Core and SQLite. Finally, Blog.Api is a thin delivery layer with controllers and authentication.

                    The payoff? You can swap SQLite for PostgreSQL, or the HTTP API for gRPC, without touching the core of the application.
                    """,
                Status = PostStatus.Published,
                Author = "admin",
                CreatedAtUtc = SeedPublishedAt2,
                UpdatedAtUtc = SeedPublishedAt2,
                PublishedAtUtc = SeedPublishedAt2
            },
            new BlogPost
            {
                Id = 3,
                Title = "A quick tour of .NET Aspire orchestration",
                Content = """
                    .NET Aspire gives local distributed apps a home: one AppHost project that describes every resource, and a dashboard that shows them all with logs, traces, and health at a glance.

                    Here, the AppHost registers the blog-api on HTTP port 10010 and HTTPS port 10011, a SQLite database resource, and a sqlite-web container on port 10001 for browsing the database in your browser.

                    Endpoints, connection strings, and service discovery are wired automatically, which means less plumbing and more building.
                    """,
                Status = PostStatus.Published,
                Author = "admin",
                CreatedAtUtc = SeedPublishedAt3,
                UpdatedAtUtc = SeedPublishedAt3,
                PublishedAtUtc = SeedPublishedAt3
            },
            new BlogPost
            {
                Id = 4,
                Title = "Draft: thoughts on writing your first blog post",
                Content = """
                    A rough draft that is only visible to the admin. Publish it from the editor when it is ready!

                    Remember: a good post has one clear idea, a friendly voice, and a call to action.
                    """,
                Status = PostStatus.Draft,
                Author = "admin",
                CreatedAtUtc = SeedDraftAt,
                UpdatedAtUtc = SeedDraftAt,
                PublishedAtUtc = null
            });
    }
}
