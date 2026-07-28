using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iBorrow.Migrations
{
    /// <inheritdoc />
    public partial class AddBookManualAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyUnavailable",
                table: "Books",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManuallyUnavailable",
                table: "Books");
        }
    }
}
