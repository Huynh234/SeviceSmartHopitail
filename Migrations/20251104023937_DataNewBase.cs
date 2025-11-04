using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class DataNewBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SleepTime",
                table: "SleepRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "WakeTime",
                table: "SleepRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SleepTime",
                table: "SleepRecords");

            migrationBuilder.DropColumn(
                name: "WakeTime",
                table: "SleepRecords");
        }
    }
}
