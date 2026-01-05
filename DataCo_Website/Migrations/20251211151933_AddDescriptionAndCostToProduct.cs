using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataCo_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionAndCostToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Product_Id",
                table: "product",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR Seq_ProductId",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "Cost",
                table: "product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Department_Id",
                table: "department",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR Seq_DepartmentId",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Category_Id",
                table: "category",
                type: "tinyint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR Seq_CategoryId",
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "product");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "product");

            migrationBuilder.AlterColumn<int>(
                name: "Product_Id",
                table: "product",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR Seq_ProductId");

            migrationBuilder.AlterColumn<int>(
                name: "Department_Id",
                table: "department",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR Seq_DepartmentId");

            migrationBuilder.AlterColumn<byte>(
                name: "Category_Id",
                table: "category",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldDefaultValueSql: "NEXT VALUE FOR Seq_CategoryId");
        }
    }
}
