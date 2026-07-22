using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YunChenShipping.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxCategoryToShippingOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaxCategoryId",
                table: "ShippingOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "ShippingOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxCategoryId",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "ShippingOrders");
        }
    }
}
