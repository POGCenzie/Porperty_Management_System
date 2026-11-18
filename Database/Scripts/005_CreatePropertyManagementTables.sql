USE [Syrento_DataBase];
GO

IF OBJECT_ID(N'dbo.Condominiums', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Condominiums
    (
        CondominiumId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Condominiums PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Condominiums_IsActive DEFAULT (1),
        LastUpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Condominiums_LastUpdatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Condominiums_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.Tenants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tenants
    (
        TenantId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tenants PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        ContactNumber NVARCHAR(100) NULL,
        EmailAddress NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Tenants_IsActive DEFAULT (1),
        LastUpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_Tenants_LastUpdatedDate DEFAULT (SYSUTCDATETIME())
    );
END;
GO

IF OBJECT_ID(N'dbo.TenantAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TenantAssignments
    (
        TenantAssignmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TenantAssignments PRIMARY KEY,
        CondominiumId INT NOT NULL,
        TenantId INT NOT NULL,
        UnitNumber NVARCHAR(50) NOT NULL,
        OwnerName NVARCHAR(200) NULL,
        OccupancyStatus NVARCHAR(30) NOT NULL,
        MeralcoSin NVARCHAR(100) NULL,
        KeyRelease NVARCHAR(100) NULL,
        AccessCardRelease NVARCHAR(100) NULL,
        RentalAmountToOwner DECIMAL(18,2) NULL,
        WithDues NVARCHAR(100) NULL,
        MonthsOfStay NVARCHAR(50) NULL,
        StartOfContract DATE NULL,
        EndOfContract DATE NULL,
        AirconMaintenanceDate DATE NULL,
        MaintenanceStatus NVARCHAR(100) NULL,
        OwnerAdvanceAndDeposit NVARCHAR(150) NULL,
        OwnerEmailAddress NVARCHAR(250) NULL,
        OwnerContactNumber NVARCHAR(100) NULL,
        MainTenantContactNumber NVARCHAR(100) NULL,
        MainTenantEmailAddress NVARCHAR(250) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_TenantAssignments_IsActive DEFAULT (1),
        LastUpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_TenantAssignments_LastUpdatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_TenantAssignments_Condominiums FOREIGN KEY (CondominiumId) REFERENCES dbo.Condominiums (CondominiumId),
        CONSTRAINT FK_TenantAssignments_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants (TenantId)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TenantAssignments_CondominiumId_UnitNumber' AND object_id = OBJECT_ID(N'dbo.TenantAssignments'))
    CREATE INDEX IX_TenantAssignments_CondominiumId_UnitNumber ON dbo.TenantAssignments (CondominiumId, UnitNumber);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TenantAssignments_TenantId' AND object_id = OBJECT_ID(N'dbo.TenantAssignments'))
    CREATE INDEX IX_TenantAssignments_TenantId ON dbo.TenantAssignments (TenantId);
GO
