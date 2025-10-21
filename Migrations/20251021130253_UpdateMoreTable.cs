using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeviceSmartHopitail.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMoreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthRecords");

            migrationBuilder.CreateTable(
                name: "BloodPressureRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    Systolic = table.Column<int>(type: "int", nullable: false),
                    Diastolic = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodPressureRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodPressureRecords_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "HoSoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BloodSugarRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    BloodSugar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodSugarRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodSugarRecords_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "HoSoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeartRateRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    HeartRate = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeartRateRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeartRateRecords_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "HoSoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemindersSleeps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TkId = table.Column<int>(type: "int", nullable: false),
                    HoursSleep = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSent = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "SleepRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    HoursSleep = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SleepRecords_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "HoSoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodPressureRecords_UserProfileId",
                table: "BloodPressureRecords",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodSugarRecords_UserProfileId",
                table: "BloodSugarRecords",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HeartRateRecords_UserProfileId",
                table: "HeartRateRecords",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RemindersSleeps_TkId",
                table: "RemindersSleeps",
                column: "TkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SleepRecords_UserProfileId",
                table: "SleepRecords",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloodPressureRecords");

            migrationBuilder.DropTable(
                name: "BloodSugarRecords");

            migrationBuilder.DropTable(
                name: "HeartRateRecords");

            migrationBuilder.DropTable(
                name: "RemindersSleeps");

            migrationBuilder.DropTable(
                name: "SleepRecords");

            migrationBuilder.CreateTable(
                name: "HealthRecords",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    BloodSugar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Diastolic = table.Column<int>(type: "int", nullable: true),
                    HeartRate = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Systolic = table.Column<int>(type: "int", nullable: true),
                    TimeSleep = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_UserProfileId",
                table: "HealthRecords",
                column: "UserProfileId");
        }
    }
}
