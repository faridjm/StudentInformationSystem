using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKeysAndGpaPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSubmissions_Enrollments_EnrollmentId1",
                table: "AssessmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_AppUserId1",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_AppUserId1",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_AppUserId1",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_AppUserId1",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentSubmissions_EnrollmentId1",
                table: "AssessmentSubmissions");

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EnrollmentId1",
                table: "AssessmentSubmissions");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentGPA",
                table: "Students",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId1",
                table: "Teachers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentGPA",
                table: "Students",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId1",
                table: "Students",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentId1",
                table: "AssessmentSubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_AppUserId1",
                table: "Teachers",
                column: "AppUserId1",
                unique: true,
                filter: "[AppUserId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Students_AppUserId1",
                table: "Students",
                column: "AppUserId1",
                unique: true,
                filter: "[AppUserId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentSubmissions_EnrollmentId1",
                table: "AssessmentSubmissions",
                column: "EnrollmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSubmissions_Enrollments_EnrollmentId1",
                table: "AssessmentSubmissions",
                column: "EnrollmentId1",
                principalTable: "Enrollments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_AppUserId1",
                table: "Students",
                column: "AppUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_AppUserId1",
                table: "Teachers",
                column: "AppUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
