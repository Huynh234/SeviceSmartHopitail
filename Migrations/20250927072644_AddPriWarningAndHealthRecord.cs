using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class AddPriWarningAndHealthRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HealthGoal",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "UnderlyingDisease",
                table: "UserProfiles",
                newName: "Address");

            migrationBuilder.CreateTable(
                name: "HealthRecords",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    HeartRate = table.Column<int>(type: "int", nullable: false),
                    BloodSugar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Systolic = table.Column<int>(type: "int", nullable: true),
                    Diastolic = table.Column<int>(type: "int", nullable: true),
                    TimeSleep = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRecords", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_HealthRecords_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "HoSoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriWarnings",
                columns: table => new
                {
                    WarningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    MinHeartRate = table.Column<int>(type: "int", nullable: true),
                    MaxHeartRate = table.Column<int>(type: "int", nullable: true),
                    MinBloodSugar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxBloodSugar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinSystolic = table.Column<int>(type: "int", nullable: true),
                    MaxSystolic = table.Column<int>(type: "int", nullable: true),
                    MinDiastolic = table.Column<int>(type: "int", nullable: true),
                    MaxDiastolic = table.Column<int>(type: "int", nullable: true),
                    MinSleep = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxSleep = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriWarnings", x => x.WarningId);
                    table.ForeignKey(
                        name: "FK_PriWarnings_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "HoSoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_UserProfileId",
                table: "HealthRecords",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PriWarnings_UserProfileId",
                table: "PriWarnings",
                column: "UserProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthRecords");

            migrationBuilder.DropTable(
                name: "PriWarnings");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "UserProfiles",
                newName: "UnderlyingDisease");

            migrationBuilder.AddColumn<string>(
                name: "HealthGoal",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
