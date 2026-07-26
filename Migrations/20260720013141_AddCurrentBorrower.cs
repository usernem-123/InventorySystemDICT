using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentBorrower : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentBorrowerId",
                table: "Items",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CurrentBorrowerId",
                table: "Items",
                column: "CurrentBorrowerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Borrowers_CurrentBorrowerId",
                table: "Items",
                column: "CurrentBorrowerId",
                principalTable: "Borrowers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Borrowers_CurrentBorrowerId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_CurrentBorrowerId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CurrentBorrowerId",
                table: "Items");
        }
    }
}
