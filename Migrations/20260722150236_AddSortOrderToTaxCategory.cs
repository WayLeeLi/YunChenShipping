using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YunChenShipping.Migrations
{
    /// <inheritdoc />
    public partial class AddSortOrderToTaxCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "TaxCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "TaxCategories");
        }
    }
}
