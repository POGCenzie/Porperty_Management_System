USE [Syrento_DataBase];
GO

IF COL_LENGTH(N'dbo.StatementsOfAccount', N'RentalAmount') IS NULL
    ALTER TABLE dbo.StatementsOfAccount ADD RentalAmount DECIMAL(18,2) NULL;
GO

IF COL_LENGTH(N'dbo.StatementsOfAccount', N'AdvanceAmount') IS NULL
    ALTER TABLE dbo.StatementsOfAccount ADD AdvanceAmount DECIMAL(18,2) NULL;
GO

IF COL_LENGTH(N'dbo.StatementsOfAccount', N'DepositAmount') IS NULL
    ALTER TABLE dbo.StatementsOfAccount ADD DepositAmount DECIMAL(18,2) NULL;
GO
