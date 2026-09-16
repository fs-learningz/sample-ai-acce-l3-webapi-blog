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
                Id = Guid.Parse("a1ec4b38-27fa-4475-9f37-20a9d008337a"),
                Title = "What's New in .NET 11",
                Content = """
                    .NET 11 is officially available, and it brings meaningful improvements across the runtime, libraries, SDK, C# language, EF Core, and the extensions ecosystem. Here's a tour of the highlights every .NET developer should know about.

                    RUNTIME

                    Runtime-native async ("Runtime Async") is the headline runtime feature. It no longer requires <EnablePreviewFeatures>true</EnablePreviewFeatures> for projects targeting net11.0, and the runtime libraries themselves are now compiled with runtime-async on. The result is cleaner stack traces, lower overhead, and a batch of performance wins: JIT-compiled runtime-async versions of synchronous task-returning methods, async continuations that skip ExecutionContext capture when there's no ambient state, tail-merged suspension points that shrink generated code, tiered compilation for runtime-async, and faster await paths thanks to task/value-task factory intrinsics and implicit tailcall improvements.

                    The JIT picked up bounds check elimination, redundant checked-context removal, devirtualization, switch expression folding, constant-folding for SequenceEqual, and redundant branch elimination, plus new Arm SVE2 intrinsics, better hardware-intrinsic cost modeling, and a single-instruction Math.BigMul on x64. NativeAOT gets faster interface dispatch through a shared dispatch helper, shrinking binaries and speeding up interface-heavy code. There's also in-process crash report logging that captures managed stack traces before the process exits (now on mobile too), new SIMD lane construction/composition APIs across the Vector<T> family, hardware FP16 instructions for Half, and AVX-VNNI-512 intrinsics. Minimum hardware requirements for x86/x64 and Arm64 were raised to unlock more modern instruction sets.

                    LIBRARIES

                    BCL updates are wide-ranging this release:
                    - Process gets run-and-capture helpers, fire-and-forget launches, SafeProcessHandle lifecycle methods, StartSuspended, TryGetProcessById, and Signal(PosixSignal) with ProcessExitStatus.
                    - New numeric types: IEEE 754 decimal floating point (Decimal32, Decimal64, Decimal128), generic Complex<T>, and INumberBase<TSelf>.TryParsePartial for delimiter-aware parsing.
                    - System.Text.Json adds generic type info retrieval, JsonNamingPolicy.PascalCase, per-member naming overrides, F# discriminated union support, Utf8JsonWriter.Reset, NDJSON-style SerializeAsyncEnumerable overloads, and converters for BFloat16 and the new decimal types.
                    - Compression gets Zstandard support in System.IO.Compression, improved Base64 APIs, CRC32 validation on ZIP entries, and a Reset() method on the streamless codecs.
                    - LINQ adds FullJoin plus tuple-returning Join/GroupJoin overloads across Enumerable, Queryable, and AsyncEnumerable.
                    - Four new zero-copy Stream types (ReadOnlyMemoryStream, WritableMemoryStream, ReadOnlySequenceStream, StringStream), an EqualityComparer<T>.Create factory, generic Random.NextInteger<T>/NextBinaryFloat<T>, and BitArray span constructors round out the collections and primitives story.
                    - Networking gains typed DNS resolution via a new DnsResolver class, HTTP request-body compression wrappers, configurable connection eviction, QuicStream.Priority for HTTP/3, and TLS hardening including an experimental caller-driven session API.
                    - Discriminated-union scaffolding (UnionAttribute, IUnion) lands in System.Runtime.CompilerServices to back the new C# union types, and MemoryCache now emits built-in OpenTelemetry metrics.

                    SDK

                    The SDK is leaner and smarter: assembly deduplication shrinks Linux/macOS installers, the CA1873 analyzer got quieter and clearer, and dotnet sln can now create and edit solution filters (.slnf). File-based apps gain #:include for splitting a single-file app across multiple files (plus compiled DLL references, NativeAOT build-output reuse, and dotnet format support), and dotnet run picks up a -e flag for passing environment variables straight from the command line.

                    dotnet watch now integrates with the Aspire app host, recovers automatically from crashes, and supports device selection for MAUI/mobile. CLI telemetry moved from Application Insights to OpenTelemetry. A NativeAOT CLI entry point now serves the full command surface out-of-process, and both the NativeAOT CLI and the MSBuild server are enabled by default. dotnet test picked up a long list of improvements, including --no-dependencies, --use-current-runtime, --timeout, --maximum-failed-tests, live in-flight test display, mobile test project support for Android/iOS/macOS/Mac Catalyst, and xUnit v3/NUnit template support. Container publishing now supports multi-architecture builds with Podman, prefers native local runtimes, and produces reproducible image digests.

                    C# 15

                    C# 15 is the default language version for net11.0 projects and introduces:
                    - Collection expression arguments: pass constructor or factory arguments, such as capacity or a comparer, via a with(...) element, e.g. [with(capacity: 10), .. values].
                    - Union types: a new union keyword defines a value that is one of several case types, with implicit conversions and compiler-checked exhaustive switches.
                    - Closed hierarchies: the closed modifier fixes a class's direct descendants at compile time within the declaring assembly, enabling exhaustive switches without a default arm.
                    - Extension indexers: declare indexers inside extension blocks so you can index into a type as if the indexer were a native member.
                    - Labeled break and continue: break and continue can now name an enclosing loop or switch label, replacing boolean-flag and goto workarounds in nested loops.
                    - Memory safety: the first step of a multi-release effort that ties "unsafe" to the operations that actually touch unmanaged memory. Declaring pointers, taking addresses, fixed, stackalloc-to-pointer conversion, and sizeof no longer require an unsafe context under the preview language version, though dereferencing still does.

                    EF CORE 11

                    EF Core 11 requires the .NET 11 SDK and runtime, and brings:
                    - Complex types and JSON columns now work with TPT/TPC inheritance, plus lambda-chained configuration such as .Property(e => e.Details.Description), and keys/indexes on nested complex-type properties.
                    - Better SQL for to-one includes: split queries no longer add unnecessary joins, and redundant ORDER BY keys from reference navigations are pruned, with benchmarks showing 22-29% query improvements.
                    - GroupBy translation and materialization improvements, a new FullJoin LINQ translation, automatic stripping of no-op CASTs, MaxBy/MinBy translation, and EF.Functions.JsonPathExists().
                    - SQL Server gets VECTOR_SEARCH() with approximate nearest-neighbor search over vector indexes, vector properties excluded from SELECT by default (up to roughly 9x faster locally and 22x over the network), full-text catalog and index creation via migrations, FREETEXTTABLE/CONTAINSTABLE support, JSON_CONTAINS translation, JSON indexes, and CLR-mapped temporal period properties.
                    - Cosmos DB adds full complex type support, richer indexing policy configuration, transactional batches by default, bulk execution, session token management, and a System.Text.Json-based serializer that replaces Newtonsoft.Json.
                    - Migrations get excludable foreign key constraints, a recorded "latest migration" ID in the model snapshot to catch merge conflicts, dotnet ef database update --add to scaffold and apply a migration in one step, offline migration removal, a .config/dotnet-ef.json configuration file, and wildcard (--context "*") support across multiple DbContexts.

                    EXTENSIONS

                    The Microsoft.Extensions ecosystem keeps pace with the platform's AI-first direction. Microsoft.Extensions.AI adds experimental routing primitives, an abstract RoutingChatClient with a concrete SemanticRoutingChatClient, plus an abstract FailoverChatClient and its OrderedFailoverChatClient implementation, making multi-model and multi-endpoint setups easier without hand-rolled retry logic. Resilience and diagnostics packages continue to mature alongside them, and Microsoft.Extensions.Diagnostics now supports declarative Activity tracing configuration via rules, so you can control what gets traced without wiring up an ActivityListener by hand.

                    WRAPPING UP

                    Between runtime-native async, a leaner and AOT-friendlier SDK, expressive new C# 15 syntax, faster EF Core queries with first-class vector and JSON search, and an increasingly AI-aware extensions ecosystem, .NET 11 is a substantial release. General availability lands in November 2026, so grab the SDK and give it a try.
                    """,
                Author = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-10)
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
