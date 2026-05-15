IF DB_ID(N'QuestECommerceDbChiriac') IS NULL
BEGIN
    CREATE DATABASE QuestECommerceDbChiriac;
END;
GO

USE QuestECommerceDbChiriac;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Email nvarchar(255) NOT NULL,
        PasswordHash nvarchar(500) NOT NULL,
        FirstName nvarchar(100) NOT NULL,
        LastName nvarchar(100) NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );
END;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Description nvarchar(1000) NOT NULL,
        Price decimal(18, 2) NOT NULL,
        FrontImageUrl nvarchar(500) NOT NULL,
        BackImageUrl nvarchar(500) NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL,
        CONSTRAINT CK_Products_Price_NonNegative CHECK (Price >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.ProductSizeStocks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductSizeStocks
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_ProductSizeStocks PRIMARY KEY,
        ProductId uniqueidentifier NOT NULL,
        Size nvarchar(5) NOT NULL,
        StockQuantity int NOT NULL,
        UpdatedAt datetimeoffset NOT NULL,
        CONSTRAINT FK_ProductSizeStocks_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id),
        CONSTRAINT UQ_ProductSizeStocks_ProductId_Size UNIQUE (ProductId, Size),
        CONSTRAINT CK_ProductSizeStocks_Size
            CHECK (Size IN (N'XS', N'S', N'M', N'L', N'XL', N'XXL')),
        CONSTRAINT CK_ProductSizeStocks_StockQuantity_NonNegative
            CHECK (StockQuantity >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        UserId uniqueidentifier NOT NULL,
        RecipientName nvarchar(150) NOT NULL,
        AddressLine nvarchar(255) NOT NULL,
        City nvarchar(100) NOT NULL,
        PostalCode nvarchar(20) NOT NULL,
        Country nvarchar(100) NOT NULL,
        TotalPrice decimal(18, 2) NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        CONSTRAINT FK_Orders_Users_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
        CONSTRAINT CK_Orders_TotalPrice_NonNegative
            CHECK (TotalPrice >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderId uniqueidentifier NOT NULL,
        ProductId uniqueidentifier NOT NULL,
        ProductName nvarchar(200) NOT NULL,
        Size nvarchar(5) NOT NULL,
        UnitPrice decimal(18, 2) NOT NULL,
        Quantity int NOT NULL,
        CONSTRAINT FK_OrderItems_Orders_OrderId
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders (Id),
        CONSTRAINT FK_OrderItems_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id),
        CONSTRAINT CK_OrderItems_Size
            CHECK (Size IN (N'XS', N'S', N'M', N'L', N'XL', N'XXL')),
        CONSTRAINT CK_OrderItems_UnitPrice_NonNegative
            CHECK (UnitPrice >= 0),
        CONSTRAINT CK_OrderItems_Quantity_Positive
            CHECK (Quantity > 0)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Orders_UserId_CreatedAt'
      AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE INDEX IX_Orders_UserId_CreatedAt
        ON dbo.Orders (UserId, CreatedAt DESC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_OrderItems_OrderId'
      AND object_id = OBJECT_ID(N'dbo.OrderItems'))
BEGIN
    CREATE INDEX IX_OrderItems_OrderId
        ON dbo.OrderItems (OrderId);
END;
GO
