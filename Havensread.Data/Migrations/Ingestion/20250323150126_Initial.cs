using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Havensread.Data.Migrations.Ingestion
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ingestion");

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => new { x.id, x.source });
                });

            migrationBuilder.CreateTable(
                name: "records",
                schema: "ingestion",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_records_documents_document_id_document_source",
                        columns: x => new { x.document_id, x.document_source },
                        principalSchema: "ingestion",
                        principalTable: "documents",
                        principalColumns: new[] { "id", "source" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_records_document_id_document_source",
                schema: "ingestion",
                table: "records",
                columns: new[] { "document_id", "document_source" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "records",
                schema: "ingestion");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "ingestion");
        }
    }
}
