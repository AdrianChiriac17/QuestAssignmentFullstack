IF DB_ID(N'QuestECommerceDbChiriac') IS NULL
BEGIN
    CREATE DATABASE QuestECommerceDbChiriac;
END;
GO

USE QuestECommerceDbChiriac;
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
