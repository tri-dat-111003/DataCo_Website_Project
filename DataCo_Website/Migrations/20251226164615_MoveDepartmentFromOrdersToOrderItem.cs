using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataCo_Website.Migrations
{
    /// <inheritdoc />
    public partial class MoveDepartmentFromOrdersToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ------------------------------------------------------
            -- 1. ORDERS: DROP FK + INDEX + COLUMN Department_Id
            ------------------------------------------------------
            IF EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_Orders_Department'
            )
            ALTER TABLE Orders DROP CONSTRAINT FK_Orders_Department;

            DROP INDEX IF EXISTS IX_Orders_DepartmentId_OrderDate ON Orders;
            DROP INDEX IF EXISTS IX_Orders_Status_OrderDate ON Orders;
            DROP INDEX IF EXISTS IX_orders_Department_Id ON Orders;

            DROP INDEX IF EXISTS IX_Orders_OrderId_Covering ON Orders;
            DROP INDEX IF EXISTS IX_Orders_OrderDate_Covering ON Orders;
            DROP INDEX IF EXISTS IX_Orders_OrderId_Lookup ON Orders;

            IF COL_LENGTH('Orders', 'Department_Id') IS NOT NULL
                ALTER TABLE Orders DROP COLUMN Department_Id;

            ------------------------------------------------------
            -- 2. CATEGORY: ADD Department_Id + FK + UPDATE
            ------------------------------------------------------
            IF COL_LENGTH('Category', 'Department_Id') IS NULL
                ALTER TABLE Category ADD Department_Id INT NULL;

            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_Category_Department'
            )
            ALTER TABLE Category
                ADD CONSTRAINT FK_Category_Department
                FOREIGN KEY (Department_Id) REFERENCES Department(Department_Id);

            UPDATE Category
            SET Department_Id =
                CASE
                    WHEN Category_Id IN (2,3,4,5,6,7,73) THEN 2
                    WHEN Category_Id IN (9,10,11,12,13,16) THEN 3
                    WHEN Category_Id IN (17,18,76,70,60,66,63) THEN 4
                    WHEN Category_Id IN (29,26,24) THEN 5
                    WHEN Category_Id IN (36,33,38,41,35,32,68,37,40,34,30,31) THEN 6
                    WHEN Category_Id IN (44,48,43,46,45,74) THEN 7
                    WHEN Category_Id IN (59) THEN 8
                    WHEN Category_Id IN (75,61,71,67) THEN 9
                    WHEN Category_Id IN (62,65,64) THEN 10
                    WHEN Category_Id IN (72) THEN 11
                    WHEN Category_Id IN (69) THEN 12
                    ELSE Department_Id
                END;

            ------------------------------------------------------
            -- 3. ORDER_ITEM: ADD Department_Id + FK + UPDATE
            ------------------------------------------------------
            IF COL_LENGTH('Order_Item', 'Department_Id') IS NULL
                ALTER TABLE Order_Item ADD Department_Id INT NULL;

            IF NOT EXISTS (
                SELECT 1 FROM sys.foreign_keys 
                WHERE name = 'FK_OrderItem_Department'
            )
            ALTER TABLE Order_Item
                ADD CONSTRAINT FK_OrderItem_Department
                FOREIGN KEY (Department_Id) REFERENCES Department(Department_Id);

            UPDATE oi
            SET oi.Department_Id = c.Department_Id
            FROM Order_Item oi
            JOIN Product p ON oi.Product_Id = p.Product_Id
            JOIN Category c ON p.Category_Id = c.Category_Id
            WHERE oi.Department_Id IS NULL;

            ------------------------------------------------------
            -- 4. CREATE INDEXES (phiên bản mới)
            ------------------------------------------------------
            CREATE NONCLUSTERED INDEX IX_Orders_OrderDate_Covering
            ON Orders(Order_Date DESC, Order_Id)
            INCLUDE (
                Customer_Id, Status,
                Order_Region, Order_Country, Order_State, Order_City,
                Market, Type_Transaction
            )
            WITH (ONLINE = ON, FILLFACTOR = 90);

            CREATE NONCLUSTERED INDEX IX_OrderItems_DepartmentId_Covering
            ON Order_Item(Department_Id)
            INCLUDE (Order_Id, Product_Id, Quantity, Sales, Total, Profit_Ratio)
            WITH (ONLINE = ON, FILLFACTOR = 90);

            CREATE NONCLUSTERED INDEX IX_Category_DepartmentId
            ON Category(Department_Id)
            INCLUDE (Category_Id, Category_Name)
            WITH (ONLINE = ON, FILLFACTOR = 90);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ------------------------------------------------------
            -- DROP INDEXES (Orders)
            ------------------------------------------------------
            DROP INDEX IF EXISTS IX_Orders_OrderId_Covering ON Orders;
            DROP INDEX IF EXISTS IX_Orders_OrderDate_Covering ON Orders;
            DROP INDEX IF EXISTS IX_Orders_OrderId_Lookup ON Orders;
            DROP INDEX IF EXISTS IX_Orders_Status_OrderDate ON Orders;

            ------------------------------------------------------
            -- DROP INDEXES (Order_Item / Category)
            ------------------------------------------------------
            DROP INDEX IF EXISTS IX_OrderItems_DepartmentId_Covering ON Order_Item;
            DROP INDEX IF EXISTS IX_Category_DepartmentId ON Category;

            ------------------------------------------------------
            -- DROP FOREIGN KEYS
            ------------------------------------------------------
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OrderItem_Department')
                ALTER TABLE Order_Item DROP CONSTRAINT FK_OrderItem_Department;

            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Category_Department')
                ALTER TABLE Category DROP CONSTRAINT FK_Category_Department;
            ");
        }
    }
}
