using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iBorrow.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBorrowerCourseAndContactNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactNo",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "Course",
                table: "Borrowers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactNo",
                table: "Borrowers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Course",
                table: "Borrowers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
