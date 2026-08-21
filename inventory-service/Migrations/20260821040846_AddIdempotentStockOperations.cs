using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventory_service.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotentStockOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockOperations",
                columns: table => new
                {
                    IdempotencyKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockOperations", x => x.IdempotencyKey);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockOperations");
        }
    }
}
