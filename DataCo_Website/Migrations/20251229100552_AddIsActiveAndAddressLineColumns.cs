using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataCo_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveAndAddressLineColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "product",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "orders",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "123 xxx, yyy, zzz");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine",
                table: "department",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "123 xxx, yyy, zzz");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "department",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "category",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "product");

            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "AddressLine",
                table: "department");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "department");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "category");

        }
    }
}
