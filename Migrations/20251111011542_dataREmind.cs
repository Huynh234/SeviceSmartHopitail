using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class dataREmind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RemindDrinkWaters");

            migrationBuilder.DropTable(
                name: "RemindersSleeps");

            migrationBuilder.DropTable(
                name: "RemindExercises");

            migrationBuilder.DropTable(
                name: "RemindTakeMedicines");

            migrationBuilder.CreateTable(
                name: "RemindAlls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TkId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeRemind = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loop = table.Column<int>(type: "int", nullable: false),
                    LastSent = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindAlls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemindAlls_TaiKhoans_TkId",
                        column: x => x.TkId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RemindAlls_TkId",
                table: "RemindAlls",
                column: "TkId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RemindAlls");

            migrationBuilder.CreateTable(
                name: "RemindDrinkWaters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TkId = table.Column<int>(type: "int", nullable: false),
                    LastSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeRemind = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindDrinkWaters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemindDrinkWaters_TaiKhoans_TkId",
                        column: x => x.TkId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemindersSleeps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TkId = table.Column<int>(type: "int", nullable: false),
                    LastSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeRemind = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindersSleeps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemindersSleeps_TaiKhoans_TkId",
                        column: x => x.TkId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemindExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TkId = table.Column<int>(type: "int", nullable: false),
                    LastSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeRemind = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemindExercises_TaiKhoans_TkId",
                        column: x => x.TkId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemindTakeMedicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TkId = table.Column<int>(type: "int", nullable: false),
                    LastSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeRemind = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindTakeMedicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemindTakeMedicines_TaiKhoans_TkId",
                        column: x => x.TkId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RemindDrinkWaters_TkId",
                table: "RemindDrinkWaters",
                column: "TkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemindersSleeps_TkId",
                table: "RemindersSleeps",
                column: "TkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemindExercises_TkId",
                table: "RemindExercises",
                column: "TkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemindTakeMedicines_TkId",
                table: "RemindTakeMedicines",
                column: "TkId",
                unique: true);
        }
    }
}
