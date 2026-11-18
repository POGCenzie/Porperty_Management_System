USE [Syrento_DataBase];
GO

IF COL_LENGTH(N'dbo.CondominiumUnits', N'OwnerName') IS NULL
    ALTER TABLE dbo.CondominiumUnits ADD OwnerName NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'StayDuration') IS NULL
    ALTER TABLE dbo.TenantAssignments ADD StayDuration INT NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'StayDurationUnit') IS NULL
    ALTER TABLE dbo.TenantAssignments ADD StayDurationUnit NVARCHAR(10) NULL;
GO

IF COL_LENGTH(N'dbo.TenantAssignments', N'MonthsOfStay') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE dbo.TenantAssignments
        SET StayDuration = TRY_CONVERT(INT, LEFT(LTRIM(MonthsOfStay), CHARINDEX('' '', LTRIM(MonthsOfStay) + '' '') - 1)),
            StayDurationUnit = CASE
                WHEN LOWER(MonthsOfStay) LIKE ''%week%'' THEN ''Weeks''
                WHEN LOWER(MonthsOfStay) LIKE ''%day%'' THEN ''Days''
                ELSE ''Months''
            END
        WHERE MonthsOfStay IS NOT NULL AND StayDuration IS NULL;
        ALTER TABLE dbo.TenantAssignments DROP COLUMN MonthsOfStay;');
END;
GO
