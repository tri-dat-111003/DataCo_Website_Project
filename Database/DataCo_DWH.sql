-- Tạo cơ sở dữ liệu
CREATE DATABASE DataCo_DWH;
GO
USE DataCo_DWH;

-- Tạo bảng DimDate
CREATE TABLE DimDate(
	DateKey int NOT NULL,
	Date datetime NOT NULL,
	DayOfWeek tinyint NOT NULL,
	DayName varchar(9) NOT NULL,
	DayOfMonth tinyint NOT NULL,
	DayOfYear smallint NOT NULL,
	WeekOfYear tinyint NOT NULL,
	MonthName varchar(9) NOT NULL,
	MonthOfYear tinyint NOT NULL,
	Quarter tinyint NOT NULL,
	Year smallint NOT NULL,
	IsAWeekday varchar(1) NOT NULL DEFAULT (('N')),
	constraint PK_DimDate PRIMARY KEY (DateKey)
)

-- Tạo bảng DimProduct
CREATE TABLE DimProduct (
    ProductKey INT IDENTITY PRIMARY KEY,
    -- attributes
    ProductId INT NOT NULL,
    ProductName VARCHAR(200) NOT NULL,
    CategoryName VARCHAR(50) NOT NULL,
    ProductPrice FLOAT NOT NULL,
    -- metadata
    RowIsCurrent BIT DEFAULT 1 NOT NULL,
    RowStartDate DATETIME DEFAULT '12/31/1899' NOT NULL,
    RowEndDate DATETIME DEFAULT '12/31/9999' NOT NULL,
    RowChangeReason NVARCHAR(200) NULL,
);

-- Tạo bảng DimCustomer
CREATE TABLE DimCustomer (
    CustomerKey INT IDENTITY PRIMARY KEY,
    -- Attributes
    CustomerId INT NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Zipcode VARCHAR(6) NOT NULL,
    Segment VARCHAR(50) NOT NULL,
    City VARCHAR(50) NOT NULL,
    State VARCHAR(3) NOT NULL,
    Country VARCHAR(50) NOT NULL,

    -- Metadata
    RowIsCurrent BIT DEFAULT 1 NOT NULL,
    RowStartDate DATETIME DEFAULT '12/31/1899' NOT NULL,
    RowEndDate DATETIME DEFAULT '12/31/9999' NOT NULL,
    RowChangeReason NVARCHAR(200) NULL,
    
);

-- Tạo bảng DimDepartment
CREATE TABLE DimDepartment (
    DepartmentKey INT IDENTITY PRIMARY KEY,
    -- Attributes
    DepartmentId INT NOT NULL,
    DepartmentName VARCHAR(50) NOT NULL,
    
    -- Metadata
    RowIsCurrent BIT DEFAULT 1 NOT NULL,
    RowStartDate DATETIME DEFAULT '12/31/1899' NOT NULL,
    RowEndDate DATETIME DEFAULT '12/31/9999' NOT NULL,
    RowChangeReason NVARCHAR(200) NULL
);

-- Tạo bảng FactSales
CREATE TABLE FactSales (
    OrderDateKey int NOT NULL,
	ProductKey INT NOT NULL,
	CustomerKey INT NOT NULL,
	DepartmentKey INT NOT NULL,
    -- Attributes
    OrderId INT NOT NULL,
	OrderItemId INT NOT NULL,
	Order_Region VARCHAR(50) NOT NULL,
    Order_City VARCHAR(50) NOT NULL,
    Order_State VARCHAR(50) NOT NULL,
    Order_Country VARCHAR(50) NOT NULL,
	Status VARCHAR(50) NOT NULL,
	Market VARCHAR(50) NOT NULL,
	Type_Transaction VARCHAR(50) NOT NULL,
	Original_Price FLOAT NOT NULL,
	Total_Price FLOAT NOT NULL,
	Discount_Amount FLOAT NOT NULL,
	Profit FLOAT NOT NULL,
    
    -- Constraints

	CONSTRAINT PK_FactSales PRIMARY KEY (ProductKey, OrderId, OrderItemId),
	CONSTRAINT FK_FactSales_Date FOREIGN KEY (OrderDateKey) REFERENCES DimDate(DateKey),
    CONSTRAINT FK_FactSales_Product FOREIGN KEY (ProductKey) REFERENCES DimProduct(ProductKey),
    CONSTRAINT FK_FactSales_Customer FOREIGN KEY (CustomerKey) REFERENCES DimCustomer(CustomerKey),
    CONSTRAINT FK_FactSales_Department FOREIGN KEY (DepartmentKey) REFERENCES DimDepartment(DepartmentKey)
);
-------------------------------------------------------------------------------------------------------------------------------
-- Tạo bảng FactShipping
CREATE TABLE FactShipping (
    OrderDateKey int NOT NULL,
	ShipDateKey int NOT NULL,
	ProductKey INT NOT NULL,
	CustomerKey INT NOT NULL,
	DepartmentKey INT NOT NULL,
    -- Attributes
    OrderId INT NOT NULL,
	OrderItemId INT NOT NULL,
	Late_Delivery_Risk BIT NOT NULL,
	Days_Scheduled INT NOT NULL,
	Days_Actual INT NOT NULL,
	Delivery_Status VARCHAR(50) NOT NULL,
	Shipping_Mode VARCHAR(50) NOT NULL,
    
    -- Constraints
	CONSTRAINT PK_FactShipping PRIMARY KEY (ProductKey, OrderId, OrderItemId),
	CONSTRAINT FK_FactShipping_OrderDate FOREIGN KEY (OrderDateKey) REFERENCES DimDate(DateKey),
	CONSTRAINT FK_FactShipping_ShipDate FOREIGN KEY (OrderDateKey) REFERENCES DimDate(DateKey),
    CONSTRAINT FK_FactShipping_Product FOREIGN KEY (ProductKey) REFERENCES DimProduct(ProductKey),
    CONSTRAINT FK_FactShipping_Customer FOREIGN KEY (CustomerKey) REFERENCES DimCustomer(CustomerKey),
    CONSTRAINT FK_FactShipping_Department FOREIGN KEY (DepartmentKey) REFERENCES DimDepartment(DepartmentKey)
);
GO
