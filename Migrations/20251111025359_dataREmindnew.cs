using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class dataREmindnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RemindAlls_TkId",
                table: "RemindAlls");

            migrationBuilder.CreateIndex(
                name: "IX_RemindAlls_TkId",
                table: "RemindAlls",
                column: "TkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RemindAlls_TkId",
                table: "RemindAlls");

            migrationBuilder.CreateIndex(
                name: "IX_RemindAlls_TkId",
                table: "RemindAlls",
                column: "TkId",
                unique: true);
        }
    }
}
