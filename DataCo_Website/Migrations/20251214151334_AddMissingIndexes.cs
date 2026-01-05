using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataCo_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.Sql(@"
                -- Orders
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_DepartmentId_OrderDate')
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_Orders_DepartmentId_OrderDate
                    ON Orders(Department_Id, Order_Date DESC)
                    INCLUDE (Customer_Id);
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status_OrderDate')
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_Orders_Status_OrderDate
                    ON Orders(Status, Order_Date DESC)
                    INCLUDE (Department_Id, Customer_Id);
                END

                -- OrderItems
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId_Total')
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId_Total
                    ON Order_Item(Order_Id)
                    INCLUDE (Total);
                END

                -- Customer
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customer_Name')
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_Customer_Name
                    ON Customer(First_Name, Last_Name);
                END

                -- AspNetUsers
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_CustomerId')
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_AspNetUsers_CustomerId
                    ON AspNetUsers(CustomerId)
                    INCLUDE (Id, Email, UserName, IsActive);
                END
            ");*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS IX_Orders_DepartmentId_OrderDate ON Orders;
                DROP INDEX IF EXISTS IX_Orders_Status_OrderDate ON Orders;
                DROP INDEX IF EXISTS IX_OrderItems_OrderId_Total ON Order_Item;
                DROP INDEX IF EXISTS IX_Customer_Name ON Customer;
                DROP INDEX IF EXISTS IX_AspNetUsers_CustomerId ON AspNetUsers;
            ");*/
        }
    }
}
