using System.Data;
using System.Globalization;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace BlogApi.Infrastructure.Persistence;

public class SqlitePostRepository(SqliteConnection connection, IUserRepository userRepository) : IPostRepository
{
    private bool _initialized;

    private const string SelectColumns =
        "id, title, content, author, author_id, created_at, updated_at, is_published, published_at";

    public IReadOnlyList<Post> GetAll()
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {SelectColumns}
            FROM posts
            ORDER BY created_at DESC
            """;

        var posts = new List<Post>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            posts.Add(MapPost(reader));
        }

        return posts;
    }

    public IReadOnlyList<Post> GetPublished()
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {SelectColumns}
            FROM posts
            WHERE is_published = 1
            ORDER BY created_at DESC
            """;

        var posts = new List<Post>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            posts.Add(MapPost(reader));
        }

        return posts;
    }

    public IReadOnlyList<Post> GetByAuthor(Guid authorId)
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {SelectColumns}
            FROM posts
            WHERE author_id = @authorId
            ORDER BY created_at DESC
            """;
        command.Parameters.AddWithValue("@authorId", authorId.ToString());

        var posts = new List<Post>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            posts.Add(MapPost(reader));
        }

        return posts;
    }

    public Post? GetById(Guid id)
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {SelectColumns}
            FROM posts
            WHERE id = @id
            """;
        command.Parameters.AddWithValue("@id", id.ToString());

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapPost(reader) : null;
    }

    public Post Add(Post post)
    {
        EnsureInitialized();

        ExecuteUpsert(post);
        return post;
    }

    public bool Update(Post post)
    {
        EnsureInitialized();

        return ExecuteUpsert(post) == 1;
    }

    public bool Delete(Guid id)
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM posts WHERE id = @id";
        command.Parameters.AddWithValue("@id", id.ToString());

        return command.ExecuteNonQuery() == 1;
    }

    private int ExecuteUpsert(Post post)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO posts (id, title, content, author, author_id, created_at, updated_at, is_published, published_at)
            VALUES (@id, @title, @content, @author, @authorId, @createdAt, @updatedAt, @isPublished, @publishedAt)
            ON CONFLICT(id) DO UPDATE SET
                title = @title,
                content = @content,
                author = @author,
                author_id = @authorId,
                created_at = @createdAt,
                updated_at = @updatedAt,
                is_published = @isPublished,
                published_at = @publishedAt
            """;
        command.Parameters.AddWithValue("@id", post.Id.ToString());
        command.Parameters.AddWithValue("@title", post.Title);
        command.Parameters.AddWithValue("@content", post.Content);
        command.Parameters.AddWithValue("@author", post.Author);
        command.Parameters.AddWithValue("@authorId", post.AuthorId.ToString());
        command.Parameters.AddWithValue("@createdAt", post.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@updatedAt", post.UpdatedAt.HasValue ? post.UpdatedAt.Value.ToString("O") : DBNull.Value);
        command.Parameters.AddWithValue("@isPublished", post.IsPublished ? 1 : 0);
        command.Parameters.AddWithValue("@publishedAt", post.PublishedAt.HasValue ? post.PublishedAt.Value.ToString("O") : DBNull.Value);

        return command.ExecuteNonQuery();
    }

    private static Post MapPost(SqliteDataReader reader) => new()
    {
        Id = Guid.ParseExact(reader.GetString(0), "D"),
        Title = reader.GetString(1),
        Content = reader.GetString(2),
        Author = reader.GetString(3),
        AuthorId = reader.IsDBNull(4) ? Guid.Empty : Guid.ParseExact(reader.GetString(4), "D"),
        CreatedAt = DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        UpdatedAt = reader.IsDBNull(6)
            ? null
            : DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        IsPublished = reader.GetInt64(7) == 1,
        PublishedAt = reader.IsDBNull(8)
            ? null
            : DateTime.Parse(reader.GetString(8), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
    };

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = """
            CREATE TABLE IF NOT EXISTS posts (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                content TEXT NOT NULL,
                author TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT,
                is_published INTEGER NOT NULL DEFAULT 0,
                published_at TEXT
            )
            """;
        createCommand.ExecuteNonQuery();

        AddColumnIfMissing("is_published", "INTEGER NOT NULL DEFAULT 0");
        AddColumnIfMissing("published_at", "TEXT");
        AddColumnIfMissing("author_id", "TEXT");

        var defaultAdminId = userRepository.GetDefaultAdmin().Id;

        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM posts";
        var count = Convert.ToInt64(countCommand.ExecuteScalar());

        if (count == 0)
        {
            foreach (var post in SeedData.Posts)
            {
                post.AuthorId = defaultAdminId;
                ExecuteUpsert(post);
            }
        }
        else
        {
            using var backfillCommand = connection.CreateCommand();
            backfillCommand.CommandText = "UPDATE posts SET author_id = @authorId WHERE author_id IS NULL OR author_id = ''";
            backfillCommand.Parameters.AddWithValue("@authorId", defaultAdminId.ToString());
            backfillCommand.ExecuteNonQuery();
        }

        _initialized = true;
    }

    private void AddColumnIfMissing(string columnName, string columnDefinition)
    {
        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = """
            SELECT COUNT(*) FROM pragma_table_info('posts')
            WHERE name = @columnName
            """;
        checkCommand.Parameters.AddWithValue("@columnName", columnName);

        if (Convert.ToInt64(checkCommand.ExecuteScalar()) == 0)
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE posts ADD COLUMN {columnName} {columnDefinition}";
            alterCommand.ExecuteNonQuery();
        }
    }

    private static class SeedData
    {
        public static readonly List<Post> Posts =
        [
            new Post
            {
                Id = Guid.Parse("3d2f1a4e-6b8c-4e2a-9f0d-1a2b3c4d5e6f"),
                Title = "What's New in .NET 11",
                Content = """
                    .NET 11 is shaping up to be another big step in the yearly release cadence, building on the performance and productivity work from .NET 10. Here's a tour of what's landing across the runtime, libraries, SDK, C# 15, EF Core, and the Microsoft.Extensions ecosystem.

                    Runtime: The JIT continues to lean harder into dynamic PGO and on-stack replacement, so hot methods get optimized earlier without waiting for a restart. Native AOT keeps shrinking the set of features that require the fallback interpreter, trimming more of the BCL cleanly, and cutting cold-start times further for CLI tools and containerized services. The GC gets more tuning for region-based server GC, reducing working-set size on memory-constrained containers, and diagnostics tooling has richer allocation and pause-time reporting out of the box.

                    Libraries: System.Text.Json ships more source-generator ergonomics, including easier polymorphic serialization and lower-allocation streaming paths. TimeProvider is now the default recommendation across more BCL APIs for testable time handling, Guid v7 (time-ordered) is easier to opt into by default, and the collection types pick up more span-based, allocation-free overloads. Regex source generation keeps closing the gap with hand-tuned engines.

                    SDK: File-based apps (`dotnet run app.cs`) mature further, making single-file scripts a genuinely productive way to prototype without a project file. Restore and build performance keep improving with the terminal logger on by default, Central Package Management is now the recommended default for new projects, and `dotnet workload` management gets simpler commands for keeping mobile and Wasm workloads up to date.

                    C# 15: Extension members graduate from preview, letting you add properties, static members, and operators to existing types, not just methods. Pattern matching keeps getting sharper with more expressive list and property patterns, `params` collections extend to more scenarios beyond arrays, and null-conditional assignment reduces boilerplate around optional member updates.

                    EF Core: EF Core 11 pushes further on complex type support, including collections of complex types and better JSON column mapping for document-style data living alongside relational tables. Bulk `ExecuteUpdate`/`ExecuteDelete` operations get more translation coverage, compiled models are faster to generate, and there's early support for vector similarity search to pair with AI-powered features.

                    Extensions: Microsoft.Extensions.AI continues its march toward a stable surface for chat clients, embeddings, and middleware like caching and function invocation, so AI features feel like a first-class citizen of the hosting model. Resilience extensions (retry, circuit breaker, timeout) get more sensible defaults, and keyed dependency injection sees expanded support across the built-in extensions packages.

                    As always, the best way to get a feel for these changes is to grab a preview SDK and try it against a real project. We'll keep this blog updated as .NET 11 moves from preview to general availability later this year.
                    """,
                Author = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Post
            {
                Id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                Title = "Getting Started with Clean Architecture",
                Content = "Clean architecture separates concerns into domain, application, infrastructure, and presentation layers.",
                Author = "Ada Lovelace",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Post
            {
                Id = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                Title = "Building Full-Stack Apps with .NET and Angular",
                Content = "A .NET Web API paired with an Angular frontend is a solid foundation for modern web applications.",
                Author = "Grace Hopper",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-1)
            }
        ];
    }
}
