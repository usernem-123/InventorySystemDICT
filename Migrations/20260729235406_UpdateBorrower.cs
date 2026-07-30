using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBorrower : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Borrowers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Borrowers");
        }
    }
}
