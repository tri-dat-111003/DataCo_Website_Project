using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataCo_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndSessionToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Carts");

            migrationBuilder.AddColumn<string>(
                name: "CurrentSessionId",
                table: "Carts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedOutDate",
                table: "CartItems",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "CartItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CartItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "InCart");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "CartItems",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId_Unique",
                table: "Carts",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_Status",
                table: "CartItems",
                columns: new[] { "CartId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CheckedOutDate",
                table: "CartItems",
                column: "CheckedOutDate",
                filter: "[CheckedOutDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_SessionId_Status",
                table: "CartItems",
                columns: new[] { "SessionId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_CustomerId_Unique",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_Status",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CheckedOutDate",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_SessionId_Status",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CurrentSessionId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "CheckedOutDate",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Carts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");
        }
    }
}
