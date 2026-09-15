using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "posts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "posts");
        }
    }
}
