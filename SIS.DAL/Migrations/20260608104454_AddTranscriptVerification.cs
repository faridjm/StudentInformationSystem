using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscriptVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TranscriptVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VerificationCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    TranscriptType = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    OfficialGrade = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    GPA = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    AttendancePercentage = table.Column<double>(type: "float", nullable: false),
                    CompletionLevel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CertificateStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscriptVerifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptVerifications_EnrollmentId",
                table: "TranscriptVerifications",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptVerifications_GroupId",
                table: "TranscriptVerifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptVerifications_StudentId",
                table: "TranscriptVerifications",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptVerifications_VerificationCode",
                table: "TranscriptVerifications",
                column: "VerificationCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranscriptVerifications");
        }
    }
}
