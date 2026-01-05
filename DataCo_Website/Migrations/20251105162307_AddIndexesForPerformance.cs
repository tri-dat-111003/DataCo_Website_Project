using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataCo_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index cho Orders
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_OrderDate",
                table: "Orders",
                columns: new[] { "Customer_Id", "Order_Date" });

            // Index cho CartItems
            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" });

            // Index cho Carts
            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId_Status",
                table: "Carts",
                columns: new[] { "CustomerId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_Carts_CustomerId_Status",
                table: "Carts");
        }
    }
}
