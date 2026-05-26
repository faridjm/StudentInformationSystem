using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearAndTermPhases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "AcademicTerms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DropDeadline",
                table: "AcademicTerms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExamPeriodEnd",
                table: "AcademicTerms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExamPeriodStart",
                table: "AcademicTerms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GradeSubmissionDeadline",
                table: "AcademicTerms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationEnd",
                table: "AcademicTerms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationStart",
                table: "AcademicTerms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicTerms_AcademicYearId",
                table: "AcademicTerms",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_Name",
                table: "AcademicYears",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicTerms_AcademicYears_AcademicYearId",
                table: "AcademicTerms",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicTerms_AcademicYears_AcademicYearId",
                table: "AcademicTerms");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropIndex(
                name: "IX_AcademicTerms_AcademicYearId",
                table: "AcademicTerms");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "AcademicTerms");

            migrationBuilder.DropColumn(
                name: "DropDeadline",
                table: "AcademicTerms");

            migrationBuilder.DropColumn(
                name: "ExamPeriodEnd",
                table: "AcademicTerms");

            migrationBuilder.DropColumn(
                name: "ExamPeriodStart",
                table: "AcademicTerms");

            migrationBuilder.DropColumn(
                name: "GradeSubmissionDeadline",
                table: "AcademicTerms");

            migrationBuilder.DropColumn(
                name: "RegistrationEnd",
                table: "AcademicTerms");

            migrationBuilder.DropColumn(
                name: "RegistrationStart",
                table: "AcademicTerms");
        }
    }
}
