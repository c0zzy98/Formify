using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Formify.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActivityLevel",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Goal",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Weight",
                table: "AppUsers",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "WorkStyle",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityLevel",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "WorkStyle",
                table: "AppUsers");
        }
    }
}
