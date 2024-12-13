CREATE TABLE [dbo].[Category] (
    [CategoryID]   NVARCHAR (10) NOT NULL,
    [CategoryName] NVARCHAR (50) NOT NULL,
    [IsHidden]     BIT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);
GO
-- Tạo trigger cho bảng danh mục
CREATE TRIGGER trgInsteadOfInsert_Category
ON Category
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @CurrentMaxID NVARCHAR(10);
    SELECT @CurrentMaxID = ISNULL(MAX(CategoryID), 'CATE000') FROM Category;

    DECLARE @NextID INT;
    SET @NextID = CAST(SUBSTRING(@CurrentMaxID, 5, 3) AS INT) + 1;

    DECLARE @NewCategories TABLE (CategoryID NVARCHAR(10), CategoryName NVARCHAR(50), IsHidden BIT);
    INSERT INTO @NewCategories (CategoryID, CategoryName, IsHidden)
    SELECT 
        'CATE' + RIGHT('000' + CAST(@NextID + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS NVARCHAR(3)), 3),
        CategoryName,
        IsHidden
    FROM inserted;

    INSERT INTO Category (CategoryID, CategoryName, IsHidden)
    SELECT CategoryID, CategoryName, IsHidden FROM @NewCategories;
END;
GO

CREATE TABLE [dbo].[ClothingType] (
    [ClothingTypeID]   NVARCHAR (10)  NOT NULL,
    [ClothingTypeName] NVARCHAR (100) NOT NULL,
    [IsHidden]         BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ClothingTypeID] ASC)
);
GO
-- Trigger cho bảng ClothingType: Tự động tạo ID mới và tránh trùng lặp
CREATE TRIGGER trgInsteadOfInsert_ClothingType
ON ClothingType
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @CurrentMaxID NVARCHAR(10);
    SELECT @CurrentMaxID = ISNULL(MAX(ClothingTypeID), 'CT000') FROM ClothingType;

    DECLARE @NextID INT;
    SET @NextID = CAST(SUBSTRING(@CurrentMaxID, 3, 3) AS INT) + 1;

    DECLARE @NewClothingTypes TABLE (ClothingTypeID NVARCHAR(10), ClothingTypeName NVARCHAR(100), IsHidden BIT);

    INSERT INTO @NewClothingTypes (ClothingTypeID, ClothingTypeName, IsHidden)
    SELECT 
        'CT' + RIGHT('000' + CAST(@NextID + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS NVARCHAR(3)), 3),
        ClothingTypeName,
        IsHidden
    FROM inserted;

    INSERT INTO ClothingType (ClothingTypeID, ClothingTypeName, IsHidden)
    SELECT ClothingTypeID, ClothingTypeName, IsHidden FROM @NewClothingTypes;
END;
GO

CREATE TABLE [dbo].[Category_ClothingType] (
    [CateCloTypeName] NVARCHAR (100) NOT NULL,
    [CategoryID]      NVARCHAR (10)  NOT NULL,
    [ClothingTypeID]  NVARCHAR (10)  NOT NULL,
    [Img]             NVARCHAR (255) NULL,
    [UrlImg]          NVARCHAR (MAX) NULL,
    [IsHidden]        BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([CategoryID] ASC, [ClothingTypeID] ASC),
    FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID]),
    FOREIGN KEY ([ClothingTypeID]) REFERENCES [dbo].[ClothingType] ([ClothingTypeID])
);
GO

CREATE TABLE [dbo].[ClothingStyle] (
    [ClothingStyleID]   NVARCHAR (10)  NOT NULL,
    [ClothingStyleName] NVARCHAR (100) NOT NULL,
    [ClothingTypeID]    NVARCHAR (10)  NOT NULL,
    [IsHidden]          BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ClothingStyleID] ASC),
    FOREIGN KEY ([ClothingTypeID]) REFERENCES [dbo].[ClothingType] ([ClothingTypeID])
);
GO
-- Trigger cho bảng ClothingStyle: Tự động tạo ID mới và tránh trùng lặp
CREATE TRIGGER trgInsteadOfInsert_ClothingStyle
ON ClothingStyle
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @CurrentMaxID NVARCHAR(10);
    SELECT @CurrentMaxID = ISNULL(MAX(ClothingStyleID), 'CS000') FROM ClothingStyle;

    DECLARE @NextID INT;
    SET @NextID = CAST(SUBSTRING(@CurrentMaxID, 3, 3) AS INT) + 1;

    DECLARE @NewClothingStyles TABLE (ClothingStyleID NVARCHAR(10), ClothingStyleName NVARCHAR(100), ClothingTypeID NVARCHAR(10), IsHidden BIT);

    INSERT INTO @NewClothingStyles (ClothingStyleID, ClothingStyleName, ClothingTypeID, IsHidden)
    SELECT 
        'CS' + RIGHT('000' + CAST(@NextID + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS NVARCHAR(3)), 3),
        ClothingStyleName,
        ClothingTypeID,
        IsHidden
    FROM inserted;

    INSERT INTO ClothingStyle (ClothingStyleID, ClothingStyleName, ClothingTypeID, IsHidden)
    SELECT ClothingStyleID, ClothingStyleName, ClothingTypeID, IsHidden FROM @NewClothingStyles;
END;
GO

CREATE TABLE [dbo].[Clothes] (
    [ClothesID]        NVARCHAR (20)   NOT NULL,
    [ClothesName]      NVARCHAR (150)  NOT NULL,
    [MainImage]        NVARCHAR (255)  NOT NULL,
    [UrlImage]         NVARCHAR (MAX)  NOT NULL,
    [Price]            DECIMAL (10, 2) NOT NULL,
    [PriceSale]        DECIMAL (10, 2) DEFAULT (NULL) NULL,
    [Description]      NVARCHAR (MAX)  NULL,
    [Fabric]           NVARCHAR (MAX)  NULL,
    [UserInstructions] NVARCHAR (MAX)  NULL,
    [CreatedAt]        DATETIME        DEFAULT (getdate()) NULL,
    [UpdatedAt]        DATETIME        DEFAULT (getdate()) NULL,
    [CategoryID]       NVARCHAR (10)   NOT NULL,
    [ClothingTypeID]   NVARCHAR (10)   NOT NULL,
    [ClothingStyleID]  NVARCHAR (10)   NOT NULL,
    [IsDeleted]        BIT             DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([ClothesID] ASC),
    FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID]),
    FOREIGN KEY ([ClothingTypeID]) REFERENCES [dbo].[ClothingType] ([ClothingTypeID]),
    FOREIGN KEY ([ClothingStyleID]) REFERENCES [dbo].[ClothingStyle] ([ClothingStyleID])
);
GO
-- Trigger cho bảng Clothes: Tự động tạo ID mới và tránh trùng lặp
CREATE TRIGGER trgInsteadOfInsert_Clothes
ON Clothes
INSTEAD OF INSERT
AS
BEGIN
    -- Bảng tạm chứa dữ liệu mới được chèn
    DECLARE @NewClothes TABLE 
    (
        RowNum INT,
        ClothesID NVARCHAR(20),
        ClothesName NVARCHAR(150),
        MainImage NVARCHAR(255),
        UrlImage NVARCHAR(MAX), -- Thêm UrlImage
        Price DECIMAL(10, 2),
        PriceSale DECIMAL(10, 2),
        Description NVARCHAR(MAX),
        Fabric NVARCHAR(MAX),
        UserInstructions NVARCHAR(MAX),
        CreatedAt DATETIME,
        UpdatedAt DATETIME,
        CategoryID NVARCHAR(10),
        ClothingTypeID NVARCHAR(10),
        ClothingStyleID NVARCHAR(10),
        IsDeleted BIT
    );

    DECLARE @CategoryID NVARCHAR(10);
    DECLARE @NextID INT;

    -- Tạo CTE để tính toán RowNum và lấy thông tin từ bảng inserted
    WITH CTE AS 
    (
        SELECT 
            ROW_NUMBER() OVER (PARTITION BY CategoryID ORDER BY ClothesName) AS RowNum,
            ClothesName, MainImage, UrlImage, Price, PriceSale, Description, Fabric, UserInstructions, 
            GETDATE() AS CreatedAt, GETDATE() AS UpdatedAt,
            CategoryID, ClothingTypeID, ClothingStyleID, 0 AS IsDeleted
        FROM inserted
    )
    
    -- Chèn dữ liệu từ CTE vào bảng tạm @NewClothes
    INSERT INTO @NewClothes (RowNum, ClothesName, MainImage, UrlImage, Price, PriceSale, Description, Fabric, UserInstructions, CreatedAt, UpdatedAt, CategoryID, ClothingTypeID, ClothingStyleID, IsDeleted)
    SELECT RowNum, ClothesName, MainImage, UrlImage, Price, PriceSale, Description, Fabric, UserInstructions, CreatedAt, UpdatedAt, CategoryID, ClothingTypeID, ClothingStyleID, IsDeleted
    FROM CTE;

    -- Lặp qua từng CategoryID để cập nhật ClothesID
    DECLARE cur CURSOR FOR SELECT DISTINCT CategoryID FROM @NewClothes;
    OPEN cur;
    FETCH NEXT FROM cur INTO @CategoryID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Tìm ID tối đa hiện tại cho CategoryID
        SELECT @NextID = ISNULL(MAX(CAST(SUBSTRING(ClothesID, LEN(@CategoryID) + 5, 3) AS INT)), 0) + 1
        FROM Clothes
        WHERE ClothesID LIKE @CategoryID + '-CLT%';

        -- Cập nhật ClothesID trong bảng tạm @NewClothes với RowNum tương ứng
        UPDATE @NewClothes
        SET ClothesID = @CategoryID + '-CLT' + RIGHT('000' + CAST(@NextID + RowNum - 1 AS NVARCHAR(3)), 3)
        WHERE CategoryID = @CategoryID;

        FETCH NEXT FROM cur INTO @CategoryID;
    END;

    CLOSE cur;
    DEALLOCATE cur;

    -- Chèn các bản ghi đã xử lý từ bảng tạm @NewClothes vào bảng Clothes
    INSERT INTO Clothes (ClothesID, ClothesName, MainImage, UrlImage, Price, PriceSale, Description, Fabric, UserInstructions, CreatedAt, UpdatedAt, CategoryID, ClothingTypeID, ClothingStyleID, IsDeleted)
    SELECT ClothesID, ClothesName, MainImage, UrlImage, Price, PriceSale, Description, Fabric, UserInstructions, CreatedAt, UpdatedAt, CategoryID, ClothingTypeID, ClothingStyleID, IsDeleted
    FROM @NewClothes;
END;
GO

CREATE TABLE [dbo].[Color] (
    [ColorID]    NVARCHAR (10)  NOT NULL,
    [ColorName]  NVARCHAR (50)  NOT NULL,
    [ImageColor] NVARCHAR (255) NOT NULL,
    [UrlImage]   NVARCHAR (MAX) NOT NULL,
    [IsHidden]   BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ColorID] ASC)
);
GO
-- Trigger cho bảng Color: Tự động tạo ID mới và tránh trùng lặp
CREATE TRIGGER trgInsteadOfInsert_Color
ON Color
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @CurrentMaxID NVARCHAR(10);
    DECLARE @NextID INT;
    
    -- Lấy ColorID lớn nhất hiện có, mặc định là 'C000' nếu chưa có
    SELECT @CurrentMaxID = ISNULL(MAX(ColorID), 'C000') FROM Color;
    
    -- Chuyển đổi phần số của ID thành số nguyên và tăng lên 1
    SET @NextID = CAST(SUBSTRING(@CurrentMaxID, 2, 3) AS INT) + 1;
    
    -- Tạo bảng tạm để lưu các bản ghi mới được chèn vào
    DECLARE @NewColors TABLE (ColorID NVARCHAR(10), ColorName NVARCHAR(50), ImageColor NVARCHAR(255), UrlImage NVARCHAR(Max), IsHidden BIT);
    
    -- Chèn các bản ghi mới từ bảng "inserted", tạo ID mới dựa trên ROW_NUMBER()
    INSERT INTO @NewColors (ColorID, ColorName, ImageColor, UrlImage, IsHidden)
    SELECT 
        'C' + RIGHT('000' + CAST(@NextID + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS NVARCHAR(3)), 3),
        ColorName,
        ImageColor,
        UrlImage,
        IsHidden
    FROM inserted;
    
    -- Chèn dữ liệu từ bảng tạm vào bảng chính
    INSERT INTO Color (ColorID, ColorName, ImageColor, UrlImage, IsHidden)
    SELECT ColorID, ColorName, ImageColor, UrlImage, IsHidden FROM @NewColors;
END;
GO

CREATE TABLE [dbo].[Size] (
    [SizeID]   NVARCHAR (10) NOT NULL,
    [SizeName] NVARCHAR (50) NOT NULL,
    [IsHidden] BIT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([SizeID] ASC)
);
GO
-- Trigger cho bảng Size: Tự động tạo ID mới và tránh trùng lặp
CREATE TRIGGER trgInsteadOfInsert_Size
ON Size
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @CurrentMaxID NVARCHAR(10);
    SELECT @CurrentMaxID = ISNULL(MAX(SizeID), 'S000') FROM Size;

    DECLARE @NextID INT;
    SET @NextID = CAST(SUBSTRING(@CurrentMaxID, 2, 3) AS INT) + 1;

    DECLARE @NewSizes TABLE (SizeID NVARCHAR(10), SizeName NVARCHAR(50), IsHidden BIT);

    INSERT INTO @NewSizes (SizeID, SizeName, IsHidden)
    SELECT 
        'S' + RIGHT('000' + CAST(@NextID + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS NVARCHAR(3)), 3),
        SizeName,
        IsHidden
    FROM inserted;

    INSERT INTO Size (SizeID, SizeName, IsHidden)
    SELECT SizeID, SizeName, IsHidden FROM @NewSizes;
END;
GO

CREATE TABLE [dbo].[Clothes_Color_Size] (
    [ClothesID] NVARCHAR (20) NOT NULL,
    [ColorID]   NVARCHAR (10) NOT NULL,
    [SizeID]    NVARCHAR (10) NOT NULL,
    [Quantity]  INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([ClothesID] ASC, [ColorID] ASC, [SizeID] ASC),
    FOREIGN KEY ([ClothesID]) REFERENCES [dbo].[Clothes] ([ClothesID]),
    FOREIGN KEY ([ColorID]) REFERENCES [dbo].[Color] ([ColorID]),
    FOREIGN KEY ([SizeID]) REFERENCES [dbo].[Size] ([SizeID])
);
GO

CREATE TABLE [dbo].[Image] (
    [ImageID]            NVARCHAR (30) NOT NULL,
    [ImageName]          VARCHAR (100) NOT NULL,
    [MainImage]          VARCHAR (255) NOT NULL,
    [UrlMainImg]         VARCHAR (MAX) NOT NULL,
    [SecondaryImage1]    VARCHAR (255) NULL,
    [UrlSecondaryImage1] VARCHAR (MAX) NULL,
    [SecondaryImage2]    VARCHAR (255) NULL,
    [UrlSecondaryImage2] VARCHAR (MAX) NULL,
    [SecondaryImage3]    VARCHAR (255) NULL,
    [UrlSecondaryImage3] VARCHAR (MAX) NULL,
    [ClothesID]          NVARCHAR (20) NOT NULL,
    [ColorID]            NVARCHAR (10) NOT NULL,
    [ImageOrder]         INT           NOT NULL,
    [IsHidden]           BIT           DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ImageID] ASC),
    UNIQUE NONCLUSTERED ([ClothesID] ASC, [ColorID] ASC),
    FOREIGN KEY ([ClothesID]) REFERENCES [dbo].[Clothes] ([ClothesID]),
    FOREIGN KEY ([ColorID]) REFERENCES [dbo].[Color] ([ColorID])
);
GO
-- Trigger cho bảng Image: Tự động tạo ID mới và tránh trùng lặp
CREATE TRIGGER trgInsteadOfInsert_Image
ON Image
INSTEAD OF INSERT
AS
BEGIN
    -- Bảng tạm để lưu trữ dữ liệu từ bảng inserted
    DECLARE @NewImages TABLE 
    (
        ImageID NVARCHAR(30),
        ImageName VARCHAR(100), 
        MainImage VARCHAR(255),
        UrlMainImg VARCHAR(MAX),
        SecondaryImage1 VARCHAR(255), 
        UrlSecondaryImage1 VARCHAR(MAX), 
        SecondaryImage2 VARCHAR(255), 
        UrlSecondaryImage2 VARCHAR(MAX), 
        SecondaryImage3 VARCHAR(255), 
        UrlSecondaryImage3 VARCHAR(MAX),
        ClothesID NVARCHAR(20), 
        ColorID NVARCHAR(10), 
        IsHidden BIT,
        ImageOrder INT -- Cột mới để lưu thứ tự
    );

    -- Lấy dữ liệu từ bảng inserted và chèn vào bảng tạm @NewImages
    INSERT INTO @NewImages (ImageName, MainImage, UrlMainImg, SecondaryImage1, UrlSecondaryImage1, SecondaryImage2, UrlSecondaryImage2, SecondaryImage3, UrlSecondaryImage3, ClothesID, ColorID, IsHidden)
    SELECT 
        ImageName, MainImage, UrlMainImg, SecondaryImage1, UrlSecondaryImage1, SecondaryImage2, UrlSecondaryImage2, SecondaryImage3, UrlSecondaryImage3, ClothesID, ColorID, IsHidden
    FROM inserted;

    -- Cập nhật ImageID và ImageOrder cho các bản ghi mới
    DECLARE @ClothesID NVARCHAR(20);
    DECLARE @ColorID NVARCHAR(10);
    DECLARE @NextID INT;
    DECLARE @ImageID NVARCHAR(30);
    DECLARE @ImageOrder INT;

    -- Sử dụng con trỏ để duyệt qua các cặp ClothesID và ColorID
    DECLARE cur CURSOR FOR SELECT DISTINCT ClothesID, ColorID FROM @NewImages;
    OPEN cur;
    FETCH NEXT FROM cur INTO @ClothesID, @ColorID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Tìm số thứ tự lớn nhất hiện tại cho ClothesID
        SELECT @ImageOrder = ISNULL(MAX(ImageOrder), 0) + 1
        FROM Image
        WHERE ClothesID = @ClothesID;

        -- Tìm ID tối đa hiện tại cho ClothesID và ColorID
        SELECT @NextID = ISNULL(MAX(CAST(SUBSTRING(ImageID, LEN(@ClothesID) + LEN(@ColorID) + 5, 3) AS INT)), 0) + 1
        FROM Image
        WHERE ImageID LIKE @ClothesID + '-' + @ColorID + '-IMG%';

        -- Cập nhật ImageID và ImageOrder trong bảng tạm @NewImages
        UPDATE n
        SET ImageID = @ClothesID + '-' + @ColorID + '-IMG' + RIGHT('000' + CAST(@NextID AS NVARCHAR(3)), 3),
            ImageOrder = @ImageOrder
        FROM @NewImages n
        WHERE n.ClothesID = @ClothesID AND n.ColorID = @ColorID;

        FETCH NEXT FROM cur INTO @ClothesID, @ColorID;
    END

    CLOSE cur;
    DEALLOCATE cur;

    -- Chèn các bản ghi đã xử lý từ bảng tạm @NewImages vào bảng Image
    INSERT INTO Image (ImageID, ImageName, MainImage, UrlMainImg, SecondaryImage1, UrlSecondaryImage1, SecondaryImage2, UrlSecondaryImage2, SecondaryImage3, UrlSecondaryImage3, ClothesID, ColorID, IsHidden, ImageOrder)
    SELECT ImageID, ImageName, MainImage, UrlMainImg, SecondaryImage1, UrlSecondaryImage1, SecondaryImage2, UrlSecondaryImage2, SecondaryImage3, UrlSecondaryImage3, ClothesID, ColorID, IsHidden, ImageOrder
    FROM @NewImages;
END;
GO

CREATE TABLE [dbo].[Users] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Username]     NVARCHAR (50)  NOT NULL,
    [Email]        NVARCHAR (100) NOT NULL,
    [PasswordHash] NVARCHAR (255) NOT NULL,
    [CreatedAt]    DATETIME       DEFAULT (getdate()) NULL,
    [IsActive]     BIT            DEFAULT ((1)) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC),
    UNIQUE NONCLUSTERED ([Username] ASC)
);
GO

CREATE TABLE [dbo].[UserRoles] (
    [Id]     INT           IDENTITY (1, 1) NOT NULL,
    [UserId] INT           NULL,
    [Role]   NVARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE TABLE [dbo].[Profiles] (
    [ProfileId]   INT            IDENTITY (1, 1) NOT NULL,
    [UserId]      INT            NULL,
    [FullName]    NVARCHAR (100) NULL,
    [PhoneNumber] NVARCHAR (15)  NULL,
    [Address]     NVARCHAR (255) NULL,
    [DateOfBirth] DATE           NULL,
    [Gender]      NVARCHAR (10)  NULL,
    PRIMARY KEY CLUSTERED ([ProfileId] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE TABLE [dbo].[Cart] (
    [CartID]      INT             IDENTITY (1, 1) NOT NULL,
    [UserID]      INT             NOT NULL,
    [CreatedAt]   DATETIME        DEFAULT (getdate()) NULL,
    [IsCompleted] BIT             DEFAULT ((0)) NULL,
    [TotalAmount] DECIMAL (18, 2) DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([CartID] ASC),
    FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([Id])
);

GO

CREATE TABLE [dbo].[CartDetail] (
    [CartDetailID] INT             IDENTITY (1, 1) NOT NULL,
    [CartID]       INT             NOT NULL,
    [ClothesID]    NVARCHAR (50)   NOT NULL,
    [ClothesName]  NVARCHAR (150)  NOT NULL,
    [MainImage]    NVARCHAR (MAX)  NOT NULL,
    [SizeName]     NVARCHAR (50)   NOT NULL,
    [ColorID]      NVARCHAR (50)   NOT NULL,
    [Quantity]     INT             NOT NULL,
    [Price]        DECIMAL (18, 2) NOT NULL,
    [TotalPrice]   AS              ([Quantity]*[Price]) PERSISTED,
    PRIMARY KEY CLUSTERED ([CartDetailID] ASC),
    FOREIGN KEY ([CartID]) REFERENCES [dbo].[Cart] ([CartID])
);

GO

CREATE TABLE [dbo].[Orders] (
    [OrderID]     INT             IDENTITY (1, 1) NOT NULL,
    [UserID]      INT             NOT NULL,
    [CreatedAt]   DATETIME        DEFAULT (getdate()) NULL,
    [Status]      NVARCHAR (50)   DEFAULT ('Pending') NULL,
    [TotalAmount] DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderID] ASC),
    FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE TABLE [dbo].[OrderDetails] (
    [OrderDetailID] INT             IDENTITY (1, 1) NOT NULL,
    [OrderID]       INT             NULL,
    [ClothesID]     NVARCHAR (50)   NULL,
    [ClothesName]   NVARCHAR (150)  NULL,
    [MainImage]     NVARCHAR (MAX)  NULL,
    [SizeName]      NVARCHAR (50)   NULL,
    [ColorID]       NVARCHAR (50)   NULL,
    [Quantity]      INT             NULL,
    [Price]         DECIMAL (18, 2) NULL,
    [TotalPrice]    AS              ([Quantity]*[Price]) PERSISTED,
    PRIMARY KEY CLUSTERED ([OrderDetailID] ASC),
    FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID])
);
GO

CREATE TABLE [dbo].[Payments] (
    [PaymentID]     INT             IDENTITY (1, 1) NOT NULL,
    [OrderID]       INT             NOT NULL,
    [PaymentMethod] NVARCHAR (50)   NULL,
    [PaymentDate]   DATETIME        DEFAULT (getdate()) NULL,
    [Amount]        DECIMAL (18, 2) NOT NULL,
    [Status]        NVARCHAR (50)   DEFAULT ('Pending') NULL,
    [TransactionID] NVARCHAR (100)  NULL,
    PRIMARY KEY CLUSTERED ([PaymentID] ASC),
    FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Orders] ([OrderID])
);
GO

CREATE TABLE [dbo].[OtpRecords] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Email]     NVARCHAR (255) NOT NULL,
    [Otp]       NVARCHAR (6)   NOT NULL,
    [CreatedAt] DATETIME       NOT NULL,
    [ExpiresAt] DATETIME       NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO



