using Microsoft.EntityFrameworkCore.Migrations;

namespace DesafioFinanceiro_Oscar.Infrastructure.Migrations
{
    public partial class ajustes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductPrice",
                table: "BuyRequests",
                newName: "ProductPrices");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductPrices",
                table: "BuyRequests",
                newName: "ProductPrice");
        }
    }
}
