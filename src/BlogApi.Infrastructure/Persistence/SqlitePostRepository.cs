using System.Data;
using System.Globalization;
using BlogApi.Application.Interfaces;
using BlogApi.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace BlogApi.Infrastructure.Persistence;

public class SqlitePostRepository(SqliteConnection connection) : IPostRepository
{
    private bool _initialized;

    public IReadOnlyList<Post> GetAll()
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, content, author, created_at, updated_at, is_published, published_at
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
        command.CommandText = """
            SELECT id, title, content, author, created_at, updated_at, is_published, published_at
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

    public Post? GetById(Guid id)
    {
        EnsureInitialized();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, content, author, created_at, updated_at, is_published, published_at
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
            INSERT INTO posts (id, title, content, author, created_at, updated_at, is_published, published_at)
            VALUES (@id, @title, @content, @author, @createdAt, @updatedAt, @isPublished, @publishedAt)
            ON CONFLICT(id) DO UPDATE SET
                title = @title,
                content = @content,
                author = @author,
                created_at = @createdAt,
                updated_at = @updatedAt,
                is_published = @isPublished,
                published_at = @publishedAt
            """;
        command.Parameters.AddWithValue("@id", post.Id.ToString());
        command.Parameters.AddWithValue("@title", post.Title);
        command.Parameters.AddWithValue("@content", post.Content);
        command.Parameters.AddWithValue("@author", post.Author);
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
        CreatedAt = DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        UpdatedAt = reader.IsDBNull(5)
            ? null
            : DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        IsPublished = reader.GetInt64(6) == 1,
        PublishedAt = reader.IsDBNull(7)
            ? null
            : DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
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

        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM posts";
        var count = Convert.ToInt64(countCommand.ExecuteScalar());

        if (count == 0)
        {
            foreach (var post in SeedData.Posts)
            {
                ExecuteUpsert(post);
            }
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
