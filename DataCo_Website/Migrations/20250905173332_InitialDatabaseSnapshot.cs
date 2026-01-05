using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataCo_Website.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabaseSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            /*migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    Category_Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Category_Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.Category_Id);
                });*/

            /*migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    Department_Id = table.Column<int>(type: "int", nullable: false),
                    Department_Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department", x => x.Department_Id);
                });*/

            /*migrationBuilder.CreateTable(
                name: "location",
                columns: table => new
                {
                    Zipcode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    City = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location", x => x.Zipcode);
                });*/

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            /*migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    Product_Id = table.Column<int>(type: "int", nullable: false),
                    Product_Name = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Category_Id = table.Column<byte>(type: "tinyint", nullable: true),
                    Product_Price = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.Product_Id);
                    table.ForeignKey(
                        name: "FK_product_category",
                        column: x => x.Category_Id,
                        principalTable: "category",
                        principalColumn: "Category_Id");
                });*/

            /*migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    Customer_Id = table.Column<int>(type: "int", nullable: false),
                    Zipcode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    Segment = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    First_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Last_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.Customer_Id);
                    table.ForeignKey(
                        name: "FK_customer_location",
                        column: x => x.Zipcode,
                        principalTable: "location",
                        principalColumn: "Zipcode");
                });*/

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Customer_Id");
                });

            /*migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Order_Id = table.Column<int>(type: "int", nullable: false),
                    Customer_Id = table.Column<int>(type: "int", nullable: true),
                    Department_Id = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Order_Region = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Order_Country = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Order_State = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Order_City = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Market = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Type_Transaction = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Order_Date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Order_Id);
                    table.ForeignKey(
                        name: "FK_orders_customer",
                        column: x => x.Customer_Id,
                        principalTable: "customer",
                        principalColumn: "Customer_Id");
                    table.ForeignKey(
                        name: "FK_orders_department",
                        column: x => x.Department_Id,
                        principalTable: "department",
                        principalColumn: "Department_Id");
                });*/

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            /*migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    Order_Item_Id = table.Column<int>(type: "int", nullable: false),
                    Order_Id = table.Column<int>(type: "int", nullable: false),
                    Product_Id = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Discount = table.Column<double>(type: "float", nullable: true),
                    Discount_Rate = table.Column<double>(type: "float", nullable: true),
                    Profit_Ratio = table.Column<double>(type: "float", nullable: true),
                    Total = table.Column<double>(type: "float", nullable: true),
                    Sales = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.Order_Item_Id);
                    table.ForeignKey(
                        name: "FK_order_item_orders",
                        column: x => x.Order_Id,
                        principalTable: "orders",
                        principalColumn: "Order_Id");
                    table.ForeignKey(
                        name: "FK_order_item_product",
                        column: x => x.Product_Id,
                        principalTable: "product",
                        principalColumn: "Product_Id");
                });*/

            /*migrationBuilder.CreateTable(
                name: "shipping",
                columns: table => new
                {
                    Order_Id = table.Column<int>(type: "int", nullable: false),
                    Order_Date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Shipping_Date = table.Column<DateTime>(type: "datetime", nullable: true),
                    Late_Delivery_Risk = table.Column<bool>(type: "bit", nullable: true),
                    Days_For_Shipment_Scheduled = table.Column<int>(type: "int", nullable: true),
                    Days_For_Shipping_Actual = table.Column<int>(type: "int", nullable: true),
                    Delivery_Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Shipping_Mode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping", x => x.Order_Id);
                    table.ForeignKey(
                        name: "FK_shipping_orders",
                        column: x => x.Order_Id,
                        principalTable: "orders",
                        principalColumn: "Order_Id");
                });*/

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CustomerId",
                table: "AspNetUsers",
                column: "CustomerId",
                unique: true,
                filter: "[CustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customer_Zipcode",
                table: "customer",
                column: "Zipcode");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_Order_Id",
                table: "order_item",
                column: "Order_Id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_Product_Id",
                table: "order_item",
                column: "Product_Id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_Customer_Id",
                table: "orders",
                column: "Customer_Id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_Department_Id",
                table: "orders",
                column: "Department_Id");

            migrationBuilder.CreateIndex(
                name: "IX_product_Category_Id",
                table: "product",
                column: "Category_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "shipping");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "department");

            migrationBuilder.DropTable(
                name: "location");
        }
    }
}
