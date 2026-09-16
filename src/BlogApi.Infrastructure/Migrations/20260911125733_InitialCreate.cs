using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "posts",
                columns: new[] { "Id", "Title", "Content", "Author", "CreatedAt", "UpdatedAt" },
                values: new object[]
                {
                    new Guid("3d2f1a4e-6b8c-4e2a-9f0d-1a2b3c4d5e6f"),
                    "What's New in .NET 11",
                    """
                    .NET 11 is shaping up to be another big step in the yearly release cadence, building on the performance and productivity work from .NET 10. Here's a tour of what's landing across the runtime, libraries, SDK, C# 15, EF Core, and the Microsoft.Extensions ecosystem.

                    Runtime: The JIT continues to lean harder into dynamic PGO and on-stack replacement, so hot methods get optimized earlier without waiting for a restart. Native AOT keeps shrinking the set of features that require the fallback interpreter, trimming more of the BCL cleanly, and cutting cold-start times further for CLI tools and containerized services. The GC gets more tuning for region-based server GC, reducing working-set size on memory-constrained containers, and diagnostics tooling has richer allocation and pause-time reporting out of the box.

                    Libraries: System.Text.Json ships more source-generator ergonomics, including easier polymorphic serialization and lower-allocation streaming paths. TimeProvider is now the default recommendation across more BCL APIs for testable time handling, Guid v7 (time-ordered) is easier to opt into by default, and the collection types pick up more span-based, allocation-free overloads. Regex source generation keeps closing the gap with hand-tuned engines.

                    SDK: File-based apps (`dotnet run app.cs`) mature further, making single-file scripts a genuinely productive way to prototype without a project file. Restore and build performance keep improving with the terminal logger on by default, Central Package Management is now the recommended default for new projects, and `dotnet workload` management gets simpler commands for keeping mobile and Wasm workloads up to date.

                    C# 15: Extension members graduate from preview, letting you add properties, static members, and operators to existing types, not just methods. Pattern matching keeps getting sharper with more expressive list and property patterns, `params` collections extend to more scenarios beyond arrays, and null-conditional assignment reduces boilerplate around optional member updates.

                    EF Core: EF Core 11 pushes further on complex type support, including collections of complex types and better JSON column mapping for document-style data living alongside relational tables. Bulk `ExecuteUpdate`/`ExecuteDelete` operations get more translation coverage, compiled models are faster to generate, and there's early support for vector similarity search to pair with AI-powered features.

                    Extensions: Microsoft.Extensions.AI continues its march toward a stable surface for chat clients, embeddings, and middleware like caching and function invocation, so AI features feel like a first-class citizen of the hosting model. Resilience extensions (retry, circuit breaker, timeout) get more sensible defaults, and keyed dependency injection sees expanded support across the built-in extensions packages.

                    As always, the best way to get a feel for these changes is to grab a preview SDK and try it against a real project. We'll keep this blog updated as .NET 11 moves from preview to general availability later this year.
                    """,
                    "admin",
                    new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc),
                    null
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posts");
        }
    }
}
