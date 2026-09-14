using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Blog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Author", "Content", "CreatedAtUtc", "PublishedAtUtc", "Status", "Title", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "admin", "Welcome to our little corner of the internet! This blog is built as a full-stack sample: an ASP.NET Core Web API following Clean Architecture on the back end, and an Angular application dressed up with daisyUI's emerald theme on the front end.\n\nBehind the scenes, everything is orchestrated with .NET Aspire. The AppHost wires up the API, a SQLite database, and a sqlite-web instance so you can peek at the data any time you like.\n\nBrowse around, read some posts, and if you are the admin, log in and start writing!", new DateTime(2026, 8, 20, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 20, 9, 0, 0, 0, DateTimeKind.Utc), 1, "Welcome to the Emerald Blog", new DateTime(2026, 8, 20, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "admin", "Clean Architecture is all about keeping business logic at the center of your solution, with infrastructure and delivery mechanisms pushed to the edges.\n\nIn this solution, Blog.Domain holds the entities and nothing else. Blog.Application depends only on the domain and defines the use cases and abstractions. Blog.Infrastructure implements those abstractions with EF Core and SQLite. Finally, Blog.Api is a thin delivery layer with controllers and authentication.\n\nThe payoff? You can swap SQLite for PostgreSQL, or the HTTP API for gRPC, without touching the core of the application.", new DateTime(2026, 8, 26, 14, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 26, 14, 30, 0, 0, DateTimeKind.Utc), 1, "Why Clean Architecture still matters", new DateTime(2026, 8, 26, 14, 30, 0, 0, DateTimeKind.Utc) },
                    { 3, "admin", ".NET Aspire gives local distributed apps a home: one AppHost project that describes every resource, and a dashboard that shows them all with logs, traces, and health at a glance.\n\nHere, the AppHost registers the blog-api on HTTP port 10010 and HTTPS port 10011, a SQLite database resource, and a sqlite-web container on port 10001 for browsing the database in your browser.\n\nEndpoints, connection strings, and service discovery are wired automatically, which means less plumbing and more building.", new DateTime(2026, 9, 2, 11, 15, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 2, 11, 15, 0, 0, DateTimeKind.Utc), 1, "A quick tour of .NET Aspire orchestration", new DateTime(2026, 9, 2, 11, 15, 0, 0, DateTimeKind.Utc) },
                    { 4, "admin", "A rough draft that is only visible to the admin. Publish it from the editor when it is ready!\n\nRemember: a good post has one clear idea, a friendly voice, and a call to action.", new DateTime(2026, 9, 12, 16, 45, 0, 0, DateTimeKind.Utc), null, 0, "Draft: thoughts on writing your first blog post", new DateTime(2026, 9, 12, 16, 45, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Status",
                table: "Posts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
