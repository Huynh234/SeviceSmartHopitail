using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatAt",
                table: "TaiKhoans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateAt",
                table: "TaiKhoans",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatAt",
                table: "TaiKhoans");

            migrationBuilder.DropColumn(
                name: "UpdateAt",
                table: "TaiKhoans");
        }
    }
}
