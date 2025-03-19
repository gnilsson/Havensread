using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Havensread.Data.Migrations.App
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.CreateTable(
                name: "authors",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "books",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    average_rating = table.Column<float>(type: "real", nullable: false),
                    ratings_count = table.Column<int>(type: "integer", nullable: false),
                    text_reviews_count = table.Column<int>(type: "integer", nullable: false),
                    isbn = table.Column<string>(type: "text", nullable: true),
                    isbn13 = table.Column<string>(type: "text", nullable: true),
                    language_code = table.Column<string>(type: "text", nullable: true),
                    num_pages = table.Column<int>(type: "integer", nullable: true),
                    publication_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    publisher = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "genres",
                schema: "app",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_genres", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "books_authors",
                schema: "app",
                columns: table => new
                {
                    authors_id = table.Column<Guid>(type: "uuid", nullable: false),
                    books_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books_authors", x => new { x.authors_id, x.books_id });
                    table.ForeignKey(
                        name: "fk_books_authors_authors_authors_id",
                        column: x => x.authors_id,
                        principalSchema: "app",
                        principalTable: "authors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_books_authors_books_books_id",
                        column: x => x.books_id,
                        principalSchema: "app",
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "books_genres",
                schema: "app",
                columns: table => new
                {
                    books_id = table.Column<Guid>(type: "uuid", nullable: false),
                    genres_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books_genres", x => new { x.books_id, x.genres_id });
                    table.ForeignKey(
                        name: "fk_books_genres_books_books_id",
                        column: x => x.books_id,
                        principalSchema: "app",
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_books_genres_genres_genres_id",
                        column: x => x.genres_id,
                        principalSchema: "app",
                        principalTable: "genres",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_books_authors_books_id",
                schema: "app",
                table: "books_authors",
                column: "books_id");

            migrationBuilder.CreateIndex(
                name: "ix_books_genres_genres_id",
                schema: "app",
                table: "books_genres",
                column: "genres_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "books_authors",
                schema: "app");

            migrationBuilder.DropTable(
                name: "books_genres",
                schema: "app");

            migrationBuilder.DropTable(
                name: "authors",
                schema: "app");

            migrationBuilder.DropTable(
                name: "books",
                schema: "app");

            migrationBuilder.DropTable(
                name: "genres",
                schema: "app");
        }
    }
}
