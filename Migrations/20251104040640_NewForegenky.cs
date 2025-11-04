using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class NewForegenky : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_QuestionLogs_TkId",
                table: "QuestionLogs",
                column: "TkId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionLogs_TaiKhoans_TkId",
                table: "QuestionLogs",
                column: "TkId",
                principalTable: "TaiKhoans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionLogs_TaiKhoans_TkId",
                table: "QuestionLogs");

            migrationBuilder.DropIndex(
                name: "IX_QuestionLogs_TkId",
                table: "QuestionLogs");
        }
    }
}
