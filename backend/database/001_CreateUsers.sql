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
