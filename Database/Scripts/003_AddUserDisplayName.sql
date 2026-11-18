USE Syrento_DataBase;
GO

IF COL_LENGTH(N'dbo.Users', N'DisplayName') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD DisplayName NVARCHAR(150) NULL;
END;
GO

UPDATE dbo.Users
SET DisplayName = Username
WHERE DisplayName IS NULL
   OR LTRIM(RTRIM(DisplayName)) = N'';
GO

ALTER TABLE dbo.Users
    ALTER COLUMN DisplayName NVARCHAR(150) NOT NULL;
GO
