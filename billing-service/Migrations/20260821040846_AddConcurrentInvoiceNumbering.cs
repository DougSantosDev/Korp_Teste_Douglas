using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace billing_service.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrentInvoiceNumbering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSequences", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InvoiceSequences",
                columns: new[] { "Id", "LastNumber" },
                values: new object[] { 1, 0 });

            migrationBuilder.Sql(
                "UPDATE InvoiceSequences SET LastNumber = (SELECT COALESCE(MAX(Number), 0) FROM Invoices) WHERE Id = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceSequences");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_Number",
                table: "Invoices");
        }
    }
}
