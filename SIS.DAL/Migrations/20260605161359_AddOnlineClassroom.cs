using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineClassroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Platform",
                table: "OnlineLessons",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MeetingUrl",
                table: "OnlineLessons",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.CreateTable(
                name: "OnlineLessonChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnlineLessonId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineLessonChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineLessonChatMessages_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineLessonChatMessages_OnlineLessons_OnlineLessonId",
                        column: x => x.OnlineLessonId,
                        principalTable: "OnlineLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessonChatMessages_OnlineLessonId_SentAt",
                table: "OnlineLessonChatMessages",
                columns: new[] { "OnlineLessonId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessonChatMessages_SenderUserId",
                table: "OnlineLessonChatMessages",
                column: "SenderUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnlineLessonChatMessages");

            migrationBuilder.AlterColumn<int>(
                name: "Platform",
                table: "OnlineLessons",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MeetingUrl",
                table: "OnlineLessons",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);
        }
    }
}
