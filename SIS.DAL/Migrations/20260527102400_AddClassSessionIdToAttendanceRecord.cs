using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddClassSessionIdToAttendanceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassSessionId",
                table: "AttendanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ClassSessionId",
                table: "AttendanceRecords",
                column: "ClassSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_ClassSessions_ClassSessionId",
                table: "AttendanceRecords",
                column: "ClassSessionId",
                principalTable: "ClassSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_ClassSessions_ClassSessionId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_ClassSessionId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ClassSessionId",
                table: "AttendanceRecords");
        }
    }
}
