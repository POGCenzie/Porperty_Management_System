IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId INT IDENTITY(1, 1) NOT NULL,
        Username NVARCHAR(100) NOT NULL,
        UserGroup NVARCHAR(10) NOT NULL
            CONSTRAINT DF_Users_UserGroup DEFAULT N'user',
        PasswordHash NVARCHAR(512) NULL,
        LastUpdatedDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_Users_LastUpdatedDate DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Users PRIMARY KEY (UserId),
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT CK_Users_UserGroup
            CHECK (UserGroup IN (N'admin', N'user'))
    );
END;
