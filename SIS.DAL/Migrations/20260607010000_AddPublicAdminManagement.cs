using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicAdminManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- PublicCourseProfiles ----------
            migrationBuilder.CreateTable(
                name: "PublicCourseProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    PublicTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublicShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublicDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    WhatYouWillLearn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CareerOutcomes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicCourseProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicCourseProfiles_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicCourseProfiles_CourseId",
                table: "PublicCourseProfiles",
                column: "CourseId",
                unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_PublicCourseProfiles_IsVisible",
                table: "PublicCourseProfiles",
                column: "IsVisible");
            migrationBuilder.CreateIndex(
                name: "IX_PublicCourseProfiles_SortOrder",
                table: "PublicCourseProfiles",
                column: "SortOrder");

            // ---------- PublicTeacherProfiles ----------
            migrationBuilder.CreateTable(
                name: "PublicTeacherProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PublicTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicTeacherProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicTeacherProfiles_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicTeacherProfiles_TeacherId",
                table: "PublicTeacherProfiles",
                column: "TeacherId",
                unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_PublicTeacherProfiles_IsVisible",
                table: "PublicTeacherProfiles",
                column: "IsVisible");
            migrationBuilder.CreateIndex(
                name: "IX_PublicTeacherProfiles_SortOrder",
                table: "PublicTeacherProfiles",
                column: "SortOrder");

            // ---------- PublicFacilities ----------
            migrationBuilder.CreateTable(
                name: "PublicFacilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_PublicFacilities", x => x.Id); });
            migrationBuilder.CreateIndex(name: "IX_PublicFacilities_IsVisible",
                table: "PublicFacilities", column: "IsVisible");
            migrationBuilder.CreateIndex(name: "IX_PublicFacilities_SortOrder",
                table: "PublicFacilities", column: "SortOrder");

            // ---------- PublicGraduateStories ----------
            migrationBuilder.CreateTable(
                name: "PublicGraduateStories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GraduatedCourse = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GraduationYear = table.Column<int>(type: "int", nullable: true),
                    CurrentPosition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Story = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_PublicGraduateStories", x => x.Id); });
            migrationBuilder.CreateIndex(name: "IX_PublicGraduateStories_IsVisible",
                table: "PublicGraduateStories", column: "IsVisible");
            migrationBuilder.CreateIndex(name: "IX_PublicGraduateStories_IsFeatured",
                table: "PublicGraduateStories", column: "IsFeatured");
            migrationBuilder.CreateIndex(name: "IX_PublicGraduateStories_SortOrder",
                table: "PublicGraduateStories", column: "SortOrder");

            // ---------- PublicFaqs ----------
            migrationBuilder.CreateTable(
                name: "PublicFaqs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_PublicFaqs", x => x.Id); });
            migrationBuilder.CreateIndex(name: "IX_PublicFaqs_IsVisible",
                table: "PublicFaqs", column: "IsVisible");
            migrationBuilder.CreateIndex(name: "IX_PublicFaqs_SortOrder",
                table: "PublicFaqs", column: "SortOrder");

            // ---------- PublicContactMessages ----------
            migrationBuilder.CreateTable(
                name: "PublicContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_PublicContactMessages", x => x.Id); });
            migrationBuilder.CreateIndex(name: "IX_PublicContactMessages_Status",
                table: "PublicContactMessages", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_PublicContactMessages_CreatedAt",
                table: "PublicContactMessages", column: "CreatedAt");
            migrationBuilder.CreateIndex(name: "IX_PublicContactMessages_Email",
                table: "PublicContactMessages", column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PublicContactMessages");
            migrationBuilder.DropTable(name: "PublicFaqs");
            migrationBuilder.DropTable(name: "PublicGraduateStories");
            migrationBuilder.DropTable(name: "PublicFacilities");
            migrationBuilder.DropTable(name: "PublicTeacherProfiles");
            migrationBuilder.DropTable(name: "PublicCourseProfiles");
        }
    }
}
