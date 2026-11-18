USE [Syrento_DataBase];
GO

IF OBJECT_ID(N'dbo.CondominiumUnits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CondominiumUnits
    (
        CondominiumUnitId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CondominiumUnits PRIMARY KEY,
        CondominiumId INT NOT NULL,
        UnitNumber NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_CondominiumUnits_IsActive DEFAULT (1),
        LastUpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_CondominiumUnits_LastUpdatedDate DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_CondominiumUnits_Condominiums FOREIGN KEY (CondominiumId)
            REFERENCES dbo.Condominiums (CondominiumId),
        CONSTRAINT UQ_CondominiumUnits_Condominium_Unit UNIQUE (CondominiumId, UnitNumber)
    );
END;
GO

INSERT INTO dbo.CondominiumUnits (CondominiumId, UnitNumber, IsActive, LastUpdatedDate)
SELECT DISTINCT assignment.CondominiumId, assignment.UnitNumber, 1, SYSUTCDATETIME()
FROM dbo.TenantAssignments AS assignment
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.CondominiumUnits AS unit
    WHERE unit.CondominiumId = assignment.CondominiumId
      AND unit.UnitNumber = assignment.UnitNumber
);
GO
