-- Create table for LetterGen admin rules
-- File: sql/create_table_lettergen_adminrules.sql
-- Run this in the ClaimsPortal database (use SSMS or sqlcmd)

-- Ensure final table name matches EF convention: dbo.LetterGenAdminRules
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LetterGen_AdminRules' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- If an earlier underscored table exists, rename it to the EF expected name
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LetterGenAdminRules' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        EXEC sp_rename 'dbo.LetterGen_AdminRules', 'LetterGenAdminRules';
        PRINT 'Renamed dbo.LetterGen_AdminRules -> dbo.LetterGenAdminRules';
    END
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LetterGenAdminRules' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.LetterGenAdminRules
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        Coverage NVARCHAR(100) NOT NULL,
        Claimant NVARCHAR(200) NOT NULL,
        IsAttorneyRepresented BIT NOT NULL CONSTRAINT DF_LG_AdminRules_IsAttorneyRepresented DEFAULT (0),
        DocumentName NVARCHAR(260) NOT NULL,
        MailTo NVARCHAR(500) NOT NULL,
        Location NVARCHAR(1000) NOT NULL,
        Priority INT NOT NULL CONSTRAINT DF_LG_AdminRules_Priority DEFAULT (100),
        Notes NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_LG_AdminRules_IsActive DEFAULT (1),
        CreatedBy NVARCHAR(100) NULL,
        CreatedAt DATETIMEOFFSET NOT NULL CONSTRAINT DF_LG_AdminRules_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIMEOFFSET NULL
    );
    CREATE INDEX IX_LG_AdminRules_Active ON dbo.LetterGenAdminRules(IsActive);
    CREATE INDEX IX_LG_AdminRules_Search ON dbo.LetterGenAdminRules(Coverage, Claimant, IsAttorneyRepresented, Priority);
END
ELSE
BEGIN
    PRINT 'Table dbo.LetterGenAdminRules already exists.';
END
GO
ELSE
BEGIN
    PRINT 'Table dbo.LetterGen_AdminRules already exists.';
END
GO
