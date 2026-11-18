USE [Syrento_DataBase];
GO

IF OBJECT_ID(N'dbo.Owners', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Owners
    (
        OwnerId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Owners PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        EmailAddress NVARCHAR(200) NULL,
        ContactNumber NVARCHAR(100) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Owners_IsActive DEFAULT (1),
        LastUpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Owners_LastUpdatedDate DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_Owners_FullName ON dbo.Owners(FullName);
END;
GO

IF COL_LENGTH(N'dbo.CondominiumUnits', N'OwnerId') IS NULL
    ALTER TABLE dbo.CondominiumUnits ADD OwnerId INT NULL;
GO

IF COL_LENGTH(N'dbo.CondominiumUnits', N'OwnerName') IS NOT NULL
BEGIN
    EXEC(N'
        INSERT INTO dbo.Owners (FullName, EmailAddress, ContactNumber, IsActive, LastUpdatedDate)
        SELECT owner_names.FullName, assignment_info.OwnerEmailAddress, assignment_info.OwnerContactNumber, 1, SYSUTCDATETIME()
        FROM
        (
            SELECT DISTINCT LTRIM(RTRIM(OwnerName)) AS FullName
            FROM dbo.CondominiumUnits
            WHERE NULLIF(LTRIM(RTRIM(OwnerName)), '''') IS NOT NULL
        ) AS owner_names
        OUTER APPLY
        (
            SELECT TOP (1) assignment.OwnerEmailAddress, assignment.OwnerContactNumber
            FROM dbo.TenantAssignments AS assignment
            WHERE LTRIM(RTRIM(assignment.OwnerName)) = owner_names.FullName
            ORDER BY assignment.LastUpdatedDate DESC
        ) AS assignment_info
        WHERE NOT EXISTS
          (
              SELECT 1 FROM dbo.Owners AS existing_owner
              WHERE existing_owner.FullName = owner_names.FullName
          );

        UPDATE unit_info
        SET OwnerId = owner_info.OwnerId
        FROM dbo.CondominiumUnits AS unit_info
        INNER JOIN dbo.Owners AS owner_info
            ON owner_info.FullName = LTRIM(RTRIM(unit_info.OwnerName));');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.CondominiumUnits') AND name = N'IX_CondominiumUnits_OwnerId')
    CREATE INDEX IX_CondominiumUnits_OwnerId ON dbo.CondominiumUnits(OwnerId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CondominiumUnits_Owners_OwnerId')
    ALTER TABLE dbo.CondominiumUnits WITH CHECK
    ADD CONSTRAINT FK_CondominiumUnits_Owners_OwnerId
        FOREIGN KEY (OwnerId) REFERENCES dbo.Owners(OwnerId);
GO

IF COL_LENGTH(N'dbo.CondominiumUnits', N'OwnerName') IS NOT NULL
    ALTER TABLE dbo.CondominiumUnits DROP COLUMN OwnerName;
GO
