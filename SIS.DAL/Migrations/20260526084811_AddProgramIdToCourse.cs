using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramIdToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicProgramId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_AcademicProgramId",
                table: "Courses",
                column: "AcademicProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AcademicPrograms_AcademicProgramId",
                table: "Courses",
                column: "AcademicProgramId",
                principalTable: "AcademicPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AcademicPrograms_AcademicProgramId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_AcademicProgramId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "AcademicProgramId",
                table: "Courses");
        }
    }
}
