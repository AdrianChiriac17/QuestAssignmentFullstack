IF DB_ID(N'QuestECommerceDbChiriac') IS NULL
BEGIN
    CREATE DATABASE QuestECommerceDbChiriac;
END;
GO

USE QuestECommerceDbChiriac;
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
