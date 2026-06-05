using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupDeliveryModeAndOnlineLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultMeetingUrl",
                table: "Groups",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultOnlinePlatform",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryMode",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OnlineLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ClassSessionId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MeetingUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineLessons_ClassSessions_ClassSessionId",
                        column: x => x.ClassSessionId,
                        principalTable: "ClassSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineLessons_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineLessons_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnlineLessonParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnlineLessonId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    IsAttendanceMarked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineLessonParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineLessonParticipants_OnlineLessons_OnlineLessonId",
                        column: x => x.OnlineLessonId,
                        principalTable: "OnlineLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineLessonParticipants_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessonParticipants_OnlineLessonId_StudentId",
                table: "OnlineLessonParticipants",
                columns: new[] { "OnlineLessonId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessonParticipants_StudentId",
                table: "OnlineLessonParticipants",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessons_ClassSessionId",
                table: "OnlineLessons",
                column: "ClassSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessons_GroupId",
                table: "OnlineLessons",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessons_Status",
                table: "OnlineLessons",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessons_TeacherId",
                table: "OnlineLessons",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnlineLessonParticipants");

            migrationBuilder.DropTable(
                name: "OnlineLessons");

            migrationBuilder.DropColumn(
                name: "DefaultMeetingUrl",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "DefaultOnlinePlatform",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "DeliveryMode",
                table: "Groups");
        }
    }
}
