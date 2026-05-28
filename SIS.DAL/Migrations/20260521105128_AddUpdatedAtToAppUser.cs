using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UpdatedAt sütunu InitialCreate-də artıq mövcuddur — skip edilir
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUsers");
        }
    }
}
