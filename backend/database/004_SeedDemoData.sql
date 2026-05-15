IF DB_ID(N'QuestECommerceDbChiriac') IS NULL
BEGIN
    CREATE DATABASE QuestECommerceDbChiriac;
END;
GO

USE QuestECommerceDbChiriac;
GO

DECLARE @Now datetimeoffset = SYSUTCDATETIME();

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Id = '9ceaa99f-50d7-4ac1-bd5e-e30a9ea7733c')
BEGIN
    UPDATE dbo.Users
    SET Email = N'adrian@mail.com',
        PasswordHash = N'PBKDF2-SHA256.100000.Rg6wRzIbW9fXZsfDeX5xrA==.GqmJb5erlBq8De7IWOxPTZj+8MkrZEC5g1uhYO8URto=',
        FirstName = N'Adrian',
        LastName = N'Chiriac'
    WHERE Id = '9ceaa99f-50d7-4ac1-bd5e-e30a9ea7733c';
END;
ELSE IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'adrian@mail.com')
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'PBKDF2-SHA256.100000.Rg6wRzIbW9fXZsfDeX5xrA==.GqmJb5erlBq8De7IWOxPTZj+8MkrZEC5g1uhYO8URto=',
        FirstName = N'Adrian',
        LastName = N'Chiriac'
    WHERE Email = N'adrian@mail.com';
END;
ELSE
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, FirstName, LastName, CreatedAt)
    VALUES
    (
        '9ceaa99f-50d7-4ac1-bd5e-e30a9ea7733c',
        N'adrian@mail.com',
        N'PBKDF2-SHA256.100000.Rg6wRzIbW9fXZsfDeX5xrA==.GqmJb5erlBq8De7IWOxPTZj+8MkrZEC5g1uhYO8URto=',
        N'Adrian',
        N'Chiriac',
        @Now
    );
END;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Id = 'f1f377c0-4235-4aaf-8ae5-b5ca6a7fa602')
BEGIN
    UPDATE dbo.Users
    SET Email = N'review@mail.com',
        PasswordHash = N'PBKDF2-SHA256.100000.nPcHcPmyv+ojeTw+E3TOiQ==.NZ21IrbhM15Y8L615RQy2O1H15Gta2Yx56/dXVm8Y/c=',
        FirstName = N'Demo',
        LastName = N'Reviewer'
    WHERE Id = 'f1f377c0-4235-4aaf-8ae5-b5ca6a7fa602';
END;
ELSE IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'review@mail.com')
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'PBKDF2-SHA256.100000.nPcHcPmyv+ojeTw+E3TOiQ==.NZ21IrbhM15Y8L615RQy2O1H15Gta2Yx56/dXVm8Y/c=',
        FirstName = N'Demo',
        LastName = N'Reviewer'
    WHERE Email = N'review@mail.com';
END;
ELSE
BEGIN
    INSERT INTO dbo.Users (Id, Email, PasswordHash, FirstName, LastName, CreatedAt)
    VALUES
    (
        'f1f377c0-4235-4aaf-8ae5-b5ca6a7fa602',
        N'review@mail.com',
        N'PBKDF2-SHA256.100000.nPcHcPmyv+ojeTw+E3TOiQ==.NZ21IrbhM15Y8L615RQy2O1H15Gta2Yx56/dXVm8Y/c=',
        N'Demo',
        N'Reviewer',
        @Now
    );
END;
GO

DECLARE @ObsoleteProducts TABLE (Id uniqueidentifier NOT NULL);

INSERT INTO @ObsoleteProducts (Id)
VALUES
    ('4e892b3f-8707-44c6-a9ec-84e6af3764ad'),
    ('c93a8f32-c49b-4a09-9c08-8ea44c0d6ac8'),
    ('ac3ce776-806e-4b3e-b98d-34c8db5c2a5c'),
    ('1fab1d9f-ef39-4dae-b2f0-06c8bfbc4bf4');

DELETE stock
FROM dbo.ProductSizeStocks AS stock
WHERE EXISTS
(
    SELECT 1
    FROM @ObsoleteProducts AS obsolete
    WHERE obsolete.Id = stock.ProductId
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.OrderItems AS orderItem
    WHERE orderItem.ProductId = stock.ProductId
);

DELETE product
FROM dbo.Products AS product
WHERE EXISTS
(
    SELECT 1
    FROM @ObsoleteProducts AS obsolete
    WHERE obsolete.Id = product.Id
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.OrderItems AS orderItem
    WHERE orderItem.ProductId = product.Id
);
GO

DECLARE @Now datetimeoffset = SYSUTCDATETIME();

DECLARE @Products TABLE
(
    Id uniqueidentifier NOT NULL,
    Name nvarchar(200) NOT NULL,
    Description nvarchar(1000) NOT NULL,
    Price decimal(18, 2) NOT NULL
);

INSERT INTO @Products (Id, Name, Description, Price)
VALUES
    (
        '1a8a95b0-e0b7-47f1-a21f-6c131809320f',
        N'Barcelona 1997 Home Shirt',
        N'Classic design home shirt as worn when Bobby Robson was manager during the 1996-97 season and led the club to a treble of cup triumphs with Ronaldo scoring 47 goals in all competitions',
        89.99
    ),
    (
        'fd7e0842-ffb7-4346-8af1-9e4022921ff2',
        N'Milan 1989 Home Shirt',
        N'A classic red and black Milan home shirt worn during the season when the side finished runners-up to Napoli in Serie A but retained their European Cup title with a 1-0 triumph over Benfica in Vienna.',
        94.99
    ),
    (
        '086fdc1c-a0ff-4f88-b855-2e3d0094933d',
        N'Netherlands 1988 Home Shirt',
        N'A bright orange Netherlands home shirt with geometric eighties patterning. As worn in qualification for Euro 88 which the side famously went on to win',
        99.99
    ),
    (
        '8813a989-df89-45de-a75d-8e96ad514148',
        N'Nigeria 1994 Home Shirt',
        N'Famous design home shirt worn at the 1994 World Cup in USA where the side beat Bulgaria and Greece to qualify for the Second Round. They were narrowly edged out by Italy after leading the game until the 88th minute when Roberto Baggio levelled the scores and then won the tie in extra-time',
        84.99
    ),
    (
        '6ebd25a0-a5db-4181-b3f8-954be7dd6983',
        N'Romania 1994 Home Shirt',
        N'A yellow Romania home shirt with tricolor details which was worn by the golden generation. Such a pity they lost on pens to the Swedes.',
        79.99
    ),
    (
        'a6ff04cf-1d30-4637-9aa2-30e42d527395',
        N'Yugoslavia 1991 Home Shirt',
        N'Home shirt worn as the side qualified for Euro 1992, but were disqualified due to the Yugoslav Wars. Denmark were chosen to take the side''s place at the tournament, and went on to become champions in a shock result. After the team were banned from Euro 1992 local factories that produced the shirt in Ireland distributed the shirt locally, meaning a lot of youth teams around Cork ended up wearing this shirt. This was the final shirt worn by the original Yugoslavia team. ',
        86.99
    );

INSERT INTO dbo.Products (Id, Name, Description, Price, FrontImageUrl, BackImageUrl, CreatedAt, UpdatedAt)
SELECT
    product.Id,
    product.Name,
    product.Description,
    product.Price,
    CONCAT(N'/images/products/', CONVERT(nvarchar(36), product.Id), N'.png'),
    CONCAT(N'/images/products/', CONVERT(nvarchar(36), product.Id), N'-back.png'),
    @Now,
    @Now
FROM @Products AS product
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Products AS existing
    WHERE existing.Id = product.Id
);
GO

DECLARE @Now datetimeoffset = SYSUTCDATETIME();

DECLARE @ProductSizeStocks TABLE
(
    ProductId uniqueidentifier NOT NULL,
    Size nvarchar(5) NOT NULL,
    StockQuantity int NOT NULL
);

INSERT INTO @ProductSizeStocks (ProductId, Size, StockQuantity)
VALUES
    ('1a8a95b0-e0b7-47f1-a21f-6c131809320f', N'XS', 2),
    ('1a8a95b0-e0b7-47f1-a21f-6c131809320f', N'S', 5),
    ('1a8a95b0-e0b7-47f1-a21f-6c131809320f', N'M', 8),
    ('1a8a95b0-e0b7-47f1-a21f-6c131809320f', N'L', 7),
    ('1a8a95b0-e0b7-47f1-a21f-6c131809320f', N'XL', 3),
    ('1a8a95b0-e0b7-47f1-a21f-6c131809320f', N'XXL', 1),
    ('fd7e0842-ffb7-4346-8af1-9e4022921ff2', N'XS', 1),
    ('fd7e0842-ffb7-4346-8af1-9e4022921ff2', N'S', 4),
    ('fd7e0842-ffb7-4346-8af1-9e4022921ff2', N'M', 7),
    ('fd7e0842-ffb7-4346-8af1-9e4022921ff2', N'L', 6),
    ('fd7e0842-ffb7-4346-8af1-9e4022921ff2', N'XL', 2),
    ('fd7e0842-ffb7-4346-8af1-9e4022921ff2', N'XXL', 0),
    ('086fdc1c-a0ff-4f88-b855-2e3d0094933d', N'XS', 0),
    ('086fdc1c-a0ff-4f88-b855-2e3d0094933d', N'S', 3),
    ('086fdc1c-a0ff-4f88-b855-2e3d0094933d', N'M', 5),
    ('086fdc1c-a0ff-4f88-b855-2e3d0094933d', N'L', 4),
    ('086fdc1c-a0ff-4f88-b855-2e3d0094933d', N'XL', 2),
    ('086fdc1c-a0ff-4f88-b855-2e3d0094933d', N'XXL', 0),
    ('8813a989-df89-45de-a75d-8e96ad514148', N'XS', 3),
    ('8813a989-df89-45de-a75d-8e96ad514148', N'S', 6),
    ('8813a989-df89-45de-a75d-8e96ad514148', N'M', 10),
    ('8813a989-df89-45de-a75d-8e96ad514148', N'L', 8),
    ('8813a989-df89-45de-a75d-8e96ad514148', N'XL', 4),
    ('8813a989-df89-45de-a75d-8e96ad514148', N'XXL', 1),
    ('6ebd25a0-a5db-4181-b3f8-954be7dd6983', N'XS', 2),
    ('6ebd25a0-a5db-4181-b3f8-954be7dd6983', N'S', 5),
    ('6ebd25a0-a5db-4181-b3f8-954be7dd6983', N'M', 9),
    ('6ebd25a0-a5db-4181-b3f8-954be7dd6983', N'L', 6),
    ('6ebd25a0-a5db-4181-b3f8-954be7dd6983', N'XL', 3),
    ('6ebd25a0-a5db-4181-b3f8-954be7dd6983', N'XXL', 1),
    ('a6ff04cf-1d30-4637-9aa2-30e42d527395', N'XS', 1),
    ('a6ff04cf-1d30-4637-9aa2-30e42d527395', N'S', 4),
    ('a6ff04cf-1d30-4637-9aa2-30e42d527395', N'M', 8),
    ('a6ff04cf-1d30-4637-9aa2-30e42d527395', N'L', 7),
    ('a6ff04cf-1d30-4637-9aa2-30e42d527395', N'XL', 3),
    ('a6ff04cf-1d30-4637-9aa2-30e42d527395', N'XXL', 0);

INSERT INTO dbo.ProductSizeStocks (Id, ProductId, Size, StockQuantity, UpdatedAt)
SELECT NEWID(), seed.ProductId, seed.Size, seed.StockQuantity, @Now
FROM @ProductSizeStocks AS seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ProductSizeStocks AS existing
    WHERE existing.ProductId = seed.ProductId
      AND existing.Size = seed.Size
);
GO
