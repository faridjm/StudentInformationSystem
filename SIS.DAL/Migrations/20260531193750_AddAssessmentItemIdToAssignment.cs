using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentItemIdToAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssessmentItemId",
                table: "Assignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_AssessmentItemId",
                table: "Assignments",
                column: "AssessmentItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_AssessmentItems_AssessmentItemId",
                table: "Assignments",
                column: "AssessmentItemId",
                principalTable: "AssessmentItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_AssessmentItems_AssessmentItemId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_AssessmentItemId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AssessmentItemId",
                table: "Assignments");
        }
    }
}
