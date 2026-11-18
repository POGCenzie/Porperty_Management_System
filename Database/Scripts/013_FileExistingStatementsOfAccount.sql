USE [Syrento_DataBase];
GO

IF OBJECT_ID(N'dbo.StatementsOfAccount', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.StatementsOfAccount', N'Status') IS NOT NULL
BEGIN
    UPDATE dbo.StatementsOfAccount
    SET Status = N'filed',
        LastUpdatedDate = SYSUTCDATETIME()
    WHERE Status <> N'filed';
END;
GO
