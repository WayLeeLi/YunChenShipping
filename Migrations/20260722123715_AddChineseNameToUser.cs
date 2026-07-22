using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YunChenShipping.Migrations
{
    /// <inheritdoc />
    public partial class AddChineseNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChineseName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChineseName",
                table: "AspNetUsers");
        }
    }
}
