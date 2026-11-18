USE [Syrento_DataBase];
GO

IF OBJECT_ID(N'dbo.StatementsOfAccount', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StatementsOfAccount
    (
        StatementOfAccountId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StatementsOfAccount PRIMARY KEY,
        SoaNumber NVARCHAR(50) NOT NULL,
        SoaType NVARCHAR(10) NOT NULL,
        TenantAssignmentId INT NOT NULL,
        StatementDate DATE NOT NULL,
        BaseAmount DECIMAL(18,2) NOT NULL,
        TotalDeductions DECIMAL(18,2) NOT NULL,
        NetAmount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        CreatedByUsername NVARCHAR(100) NOT NULL,
        LastUpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_StatementsOfAccount_LastUpdatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_StatementsOfAccount_SoaType CHECK (SoaType IN ('owner', 'tenant')),
        CONSTRAINT UQ_StatementsOfAccount_SoaNumber UNIQUE (SoaNumber),
        CONSTRAINT FK_StatementsOfAccount_TenantAssignments FOREIGN KEY (TenantAssignmentId)
            REFERENCES dbo.TenantAssignments (TenantAssignmentId)
    );
END;
GO

IF OBJECT_ID(N'dbo.SoaLineItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SoaLineItems
    (
        SoaLineItemId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SoaLineItems PRIMARY KEY,
        StatementOfAccountId INT NOT NULL,
        Description NVARCHAR(200) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Remarks NVARCHAR(300) NULL,
        SortOrder INT NOT NULL,
        CONSTRAINT FK_SoaLineItems_StatementsOfAccount FOREIGN KEY (StatementOfAccountId)
            REFERENCES dbo.StatementsOfAccount (StatementOfAccountId) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StatementsOfAccount_TenantAssignmentId' AND object_id = OBJECT_ID(N'dbo.StatementsOfAccount'))
    CREATE INDEX IX_StatementsOfAccount_TenantAssignmentId ON dbo.StatementsOfAccount (TenantAssignmentId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SoaLineItems_StatementOfAccountId' AND object_id = OBJECT_ID(N'dbo.SoaLineItems'))
    CREATE INDEX IX_SoaLineItems_StatementOfAccountId ON dbo.SoaLineItems (StatementOfAccountId);
GO
