USE [Syrento_DataBase];
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerAdvanceAmount') IS NULL
    ALTER TABLE dbo.TenantAssignments ADD OwnerAdvanceAmount DECIMAL(18,2) NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerDepositAmount') IS NULL
    ALTER TABLE dbo.TenantAssignments ADD OwnerDepositAmount DECIMAL(18,2) NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerAdvanceAndDeposit') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE dbo.TenantAssignments
        SET OwnerAdvanceAmount = COALESCE(
            OwnerAdvanceAmount,
            TRY_CONVERT(DECIMAL(18,2), OwnerAdvanceAndDeposit));
        ALTER TABLE dbo.TenantAssignments DROP COLUMN OwnerAdvanceAndDeposit;');
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns AS column_info
    INNER JOIN sys.types AS type_info ON column_info.user_type_id = type_info.user_type_id
    WHERE column_info.object_id = OBJECT_ID(N'dbo.TenantAssignments')
      AND column_info.name = N'KeyRelease'
      AND type_info.name <> N'int')
BEGIN
    ALTER TABLE dbo.TenantAssignments ADD KeyReleaseNumber INT NULL;
    EXEC(N'UPDATE dbo.TenantAssignments SET KeyReleaseNumber = TRY_CONVERT(INT, KeyRelease);');
    ALTER TABLE dbo.TenantAssignments DROP COLUMN KeyRelease;
    EXEC sp_rename N'dbo.TenantAssignments.KeyReleaseNumber', N'KeyRelease', N'COLUMN';
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns AS column_info
    INNER JOIN sys.types AS type_info ON column_info.user_type_id = type_info.user_type_id
    WHERE column_info.object_id = OBJECT_ID(N'dbo.TenantAssignments')
      AND column_info.name = N'AccessCardRelease'
      AND type_info.name <> N'int')
BEGIN
    ALTER TABLE dbo.TenantAssignments ADD AccessCardReleaseNumber INT NULL;
    EXEC(N'UPDATE dbo.TenantAssignments SET AccessCardReleaseNumber = TRY_CONVERT(INT, AccessCardRelease);');
    ALTER TABLE dbo.TenantAssignments DROP COLUMN AccessCardRelease;
    EXEC sp_rename N'dbo.TenantAssignments.AccessCardReleaseNumber', N'AccessCardRelease', N'COLUMN';
END;
GO
