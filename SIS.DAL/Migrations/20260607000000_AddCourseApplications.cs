using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ConvertedStudentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseApplications_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourseApplications_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourseApplications_Students_ConvertedStudentId",
                        column: x => x.ConvertedStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseApplications_CourseId",
                table: "CourseApplications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseApplications_GroupId",
                table: "CourseApplications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseApplications_ConvertedStudentId",
                table: "CourseApplications",
                column: "ConvertedStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseApplications_Status",
                table: "CourseApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CourseApplications_CreatedAt",
                table: "CourseApplications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CourseApplications_Email",
                table: "CourseApplications",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseApplications");
        }
    }
}
