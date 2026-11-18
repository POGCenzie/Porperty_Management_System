USE Syrento_DataBase;
GO

IF COL_LENGTH(N'dbo.Users', N'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD IsActive BIT NOT NULL
            CONSTRAINT DF_Users_IsActive DEFAULT (1) WITH VALUES;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints defaultConstraint
    INNER JOIN sys.columns columnDefinition
        ON columnDefinition.default_object_id = defaultConstraint.object_id
    WHERE defaultConstraint.parent_object_id = OBJECT_ID(N'dbo.Users')
      AND columnDefinition.name = N'IsActive'
)
BEGIN
    ALTER TABLE dbo.Users
        ADD CONSTRAINT DF_Users_IsActive DEFAULT (1) FOR IsActive;
END;
GO
