using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TkId",
                table: "QuestionLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TkId",
                table: "QuestionLogs");
        }
    }
}
