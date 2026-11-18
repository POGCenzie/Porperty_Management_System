USE [Syrento_DataBase];
GO

IF OBJECT_ID(N'dbo.StatementsOfAccount', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.StatementsOfAccount', N'RentalAmount') IS NULL
    ALTER TABLE dbo.StatementsOfAccount ADD RentalAmount DECIMAL(18,2) NULL;
GO

IF OBJECT_ID(N'dbo.StatementsOfAccount', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.StatementsOfAccount', N'AdvanceAmount') IS NULL
    ALTER TABLE dbo.StatementsOfAccount ADD AdvanceAmount DECIMAL(18,2) NULL;
GO

IF OBJECT_ID(N'dbo.StatementsOfAccount', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.StatementsOfAccount', N'DepositAmount') IS NULL
    ALTER TABLE dbo.StatementsOfAccount ADD DepositAmount DECIMAL(18,2) NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerAdvanceMonths') IS NULL
    ALTER TABLE dbo.TenantAssignments ADD OwnerAdvanceMonths INT NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerDepositMonths') IS NULL
    ALTER TABLE dbo.TenantAssignments ADD OwnerDepositMonths INT NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerAdvanceAmount') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        UPDATE dbo.TenantAssignments
        SET OwnerAdvanceMonths = CASE
                WHEN OwnerAdvanceAmount IS NULL OR OwnerAdvanceAmount <= 0 THEN NULL
                WHEN OwnerAdvanceAmount <= 120 THEN CONVERT(int, ROUND(OwnerAdvanceAmount, 0))
                WHEN RentalAmountToOwner > 0 THEN CONVERT(int, ROUND(OwnerAdvanceAmount / RentalAmountToOwner, 0))
                ELSE NULL
            END;';
END;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerDepositAmount') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        UPDATE dbo.TenantAssignments
        SET OwnerDepositMonths = CASE
                WHEN OwnerDepositAmount IS NULL OR OwnerDepositAmount <= 0 THEN NULL
                WHEN OwnerDepositAmount <= 120 THEN CONVERT(int, ROUND(OwnerDepositAmount, 0))
                WHEN RentalAmountToOwner > 0 THEN CONVERT(int, ROUND(OwnerDepositAmount / RentalAmountToOwner, 0))
                ELSE NULL
            END;';
END;
GO

IF OBJECT_ID(N'dbo.StatementsOfAccount', N'U') IS NOT NULL
BEGIN
    UPDATE statement_info
    SET RentalAmount = NULLIF(amount_info.Rent, 0),
        AdvanceAmount = amount_info.Rent * COALESCE(assignment.OwnerAdvanceMonths, 0),
        DepositAmount = amount_info.Rent * COALESCE(assignment.OwnerDepositMonths, 0),
        BaseAmount = CASE
            WHEN statement_info.SoaType = N'owner'
                THEN amount_info.Rent * (COALESCE(assignment.OwnerAdvanceMonths, 0) + COALESCE(assignment.OwnerDepositMonths, 0))
            ELSE amount_info.Rent * COALESCE(assignment.OwnerDepositMonths, 0)
        END,
        NetAmount = CASE
            WHEN statement_info.SoaType = N'owner'
                THEN amount_info.Rent * (COALESCE(assignment.OwnerAdvanceMonths, 0) + COALESCE(assignment.OwnerDepositMonths, 0))
            ELSE amount_info.Rent * COALESCE(assignment.OwnerDepositMonths, 0)
        END - statement_info.TotalDeductions,
        LastUpdatedDate = SYSUTCDATETIME()
    FROM dbo.StatementsOfAccount AS statement_info
    INNER JOIN dbo.TenantAssignments AS assignment
        ON assignment.TenantAssignmentId = statement_info.TenantAssignmentId
    CROSS APPLY
    (
        SELECT COALESCE(statement_info.RentalAmount, assignment.RentalAmountToOwner, 0) AS Rent
    ) AS amount_info;
END;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerAdvanceAmount') IS NOT NULL
    EXEC sys.sp_executesql N'ALTER TABLE dbo.TenantAssignments DROP COLUMN OwnerAdvanceAmount;';
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'OwnerDepositAmount') IS NOT NULL
    EXEC sys.sp_executesql N'ALTER TABLE dbo.TenantAssignments DROP COLUMN OwnerDepositAmount;';
GO
