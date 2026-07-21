using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YunChenShipping.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShippingOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AccountingApproved",
                table: "ShippingOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountingApprovedAt",
                table: "ShippingOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountingName",
                table: "ShippingOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HandlerApproved",
                table: "ShippingOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "HandlerApprovedAt",
                table: "ShippingOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ManagerApproved",
                table: "ShippingOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerApprovedAt",
                table: "ShippingOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerName",
                table: "ShippingOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartNo",
                table: "ShippingOrderDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "ShippingOrderDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ShippingOrderDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountingApproved",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "AccountingApprovedAt",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "AccountingName",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "HandlerApproved",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "HandlerApprovedAt",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "ManagerApproved",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "ManagerApprovedAt",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "ManagerName",
                table: "ShippingOrders");

            migrationBuilder.DropColumn(
                name: "PartNo",
                table: "ShippingOrderDetails");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "ShippingOrderDetails");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ShippingOrderDetails");
        }
    }
}
