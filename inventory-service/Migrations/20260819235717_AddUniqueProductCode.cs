using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventory_service.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueProductCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Code",
                table: "Products");
        }
    }
}
