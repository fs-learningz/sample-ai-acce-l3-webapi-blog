using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostPublishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "posts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "posts",
                keyColumn: "Id",
                keyValue: new Guid("3d2f1a4e-6b8c-4e2a-9f0d-1a2b3c4d5e6f"),
                columns: new[] { "IsPublished", "PublishedAt" },
                values: new object[] { true, new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "posts");
        }
    }
}
