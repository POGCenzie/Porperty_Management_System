USE [Syrento_DataBase];
GO

IF COL_LENGTH(N'dbo.CondominiumUnits', N'MeralcoSin') IS NULL
BEGIN
    ALTER TABLE dbo.CondominiumUnits
        ADD MeralcoSin NVARCHAR(100) NULL;
END;
GO

UPDATE unit_info
SET unit_info.MeralcoSin = assignment_info.MeralcoSin
FROM dbo.CondominiumUnits AS unit_info
OUTER APPLY
(
    SELECT TOP (1) assignment.MeralcoSin
    FROM dbo.TenantAssignments AS assignment
    WHERE assignment.CondominiumId = unit_info.CondominiumId
      AND assignment.UnitNumber = unit_info.UnitNumber
      AND NULLIF(LTRIM(RTRIM(assignment.MeralcoSin)), N'') IS NOT NULL
    ORDER BY assignment.LastUpdatedDate DESC, assignment.TenantAssignmentId DESC
) AS assignment_info
WHERE NULLIF(LTRIM(RTRIM(unit_info.MeralcoSin)), N'') IS NULL
  AND assignment_info.MeralcoSin IS NOT NULL;
GO
