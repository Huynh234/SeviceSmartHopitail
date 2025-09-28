using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class Newtextchunk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IcdCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chapter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceVolume = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IcdCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndexTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IcdCodeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndexTerms_IcdCodes_IcdCodeId",
                        column: x => x.IcdCodeId,
                        principalTable: "IcdCodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TextChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IcdCodeId = table.Column<int>(type: "int", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Embedding = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    EmbeddingCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextChunks_IcdCodes_IcdCodeId",
                        column: x => x.IcdCodeId,
                        principalTable: "IcdCodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexTerms_IcdCodeId",
                table: "IndexTerms",
                column: "IcdCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TextChunks_IcdCodeId",
                table: "TextChunks",
                column: "IcdCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndexTerms");

            migrationBuilder.DropTable(
                name: "QuestionLogs");

            migrationBuilder.DropTable(
                name: "TextChunks");

            migrationBuilder.DropTable(
                name: "IcdCodes");
        }
    }
}
