using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        private static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dev-only seed credentials: username "admin", password "Admin123!" — change after first login.
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "Username", "PasswordHash", "Role", "CreatedAt" },
                values: new object[]
                {
                    AdminUserId,
                    "admin",
                    "100000.NjtO2oZ2PRmbBPB0QOBRLw==.93Eu9evLEZ1l0pwqyXU+x4BcJYtTExdniJVTT9Ns058=",
                    "Admin",
                    new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            migrationBuilder.InsertData(
                table: "posts",
                columns: new[] { "Id", "Title", "Content", "UserId", "CreatedAt", "UpdatedAt", "IsPublished", "PublishedAt" },
                values: new object[]
                {
                    Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                    "Getting Started with Clean Architecture",
                    "Clean architecture separates concerns into domain, application, infrastructure, and presentation layers.",
                    AdminUserId,
                    new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc),
                    null,
                    true,
                    new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc)
                });

            migrationBuilder.InsertData(
                table: "posts",
                columns: new[] { "Id", "Title", "Content", "UserId", "CreatedAt", "UpdatedAt", "IsPublished", "PublishedAt" },
                values: new object[]
                {
                    Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                    "Building Full-Stack Apps with .NET and Angular",
                    "A .NET Web API paired with an Angular frontend is a solid foundation for modern web applications.",
                    AdminUserId,
                    new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc),
                    null,
                    true,
                    new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "posts",
                keyColumn: "Id",
                keyValue: Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"));

            migrationBuilder.DeleteData(
                table: "posts",
                keyColumn: "Id",
                keyValue: Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: AdminUserId);
        }
    }
}
