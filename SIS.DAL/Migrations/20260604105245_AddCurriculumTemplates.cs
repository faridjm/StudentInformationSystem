using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassSessionId",
                table: "AssessmentItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CurriculumTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumTemplates_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurriculumTemplateId = table.Column<int>(type: "int", nullable: false),
                    SessionNumber = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HasAssessment = table.Column<bool>(type: "bit", nullable: false),
                    AssessmentTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AssessmentType = table.Column<int>(type: "int", nullable: true),
                    WeightPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MaxScore = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumTemplateItems_CurriculumTemplates_CurriculumTemplateId",
                        column: x => x.CurriculumTemplateId,
                        principalTable: "CurriculumTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentItems_ClassSessionId",
                table: "AssessmentItems",
                column: "ClassSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTemplateItems_CurriculumTemplateId_SessionNumber",
                table: "CurriculumTemplateItems",
                columns: new[] { "CurriculumTemplateId", "SessionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumTemplates_CourseId",
                table: "CurriculumTemplates",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentItems_ClassSessions_ClassSessionId",
                table: "AssessmentItems",
                column: "ClassSessionId",
                principalTable: "ClassSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentItems_ClassSessions_ClassSessionId",
                table: "AssessmentItems");

            migrationBuilder.DropTable(
                name: "CurriculumTemplateItems");

            migrationBuilder.DropTable(
                name: "CurriculumTemplates");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentItems_ClassSessionId",
                table: "AssessmentItems");

            migrationBuilder.DropColumn(
                name: "ClassSessionId",
                table: "AssessmentItems");
        }
    }
}
