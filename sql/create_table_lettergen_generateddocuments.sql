-- Creates dbo.LetterGen_GeneratedDocuments to store generated PDF metadata and optional binary content
-- Run in the ClaimsPortal database (backup first in production)

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'dbo' AND t.name = 'LetterGen_GeneratedDocuments')
BEGIN
    CREATE TABLE dbo.LetterGen_GeneratedDocuments (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        RuleId BIGINT NULL,
        ClaimNumber NVARCHAR(100) NULL,
        SubClaimId BIGINT NULL,
        SubClaimFeatureNumber INT NULL,
        DocumentNumber NVARCHAR(100) NULL,
        FileName NVARCHAR(260) NOT NULL,
        StorageProvider NVARCHAR(50) NOT NULL DEFAULT('filesystem'), -- filesystem, azure, s3, db
        StoragePath NVARCHAR(400) NULL, -- full path when stored on filesystem
        Content VARBINARY(MAX) NULL, -- optional binary when stored in-db
        ContentType NVARCHAR(100) NOT NULL DEFAULT('application/pdf'),
        FileSize BIGINT NULL,
        Sha256Hash NVARCHAR(128) NULL,
        MailTo NVARCHAR(400) NULL,
        MailStatus NVARCHAR(50) NULL,
        SentAt DATETIMEOFFSET NULL,
        CreatedBy NVARCHAR(100) NULL,
        CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIMEOFFSET NULL
    );

    -- optional FK to LetterGen_AdminRules if present
    IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'dbo' AND t.name = 'LetterGen_AdminRules')
    BEGIN
        ALTER TABLE dbo.LetterGen_GeneratedDocuments
        ADD CONSTRAINT FK_LG_GeneratedDocuments_AdminRule FOREIGN KEY (RuleId) REFERENCES dbo.LetterGen_AdminRules(Id);
    END

    CREATE INDEX IX_LG_GeneratedDocuments_FileName ON dbo.LetterGen_GeneratedDocuments(FileName);
    CREATE INDEX IX_LG_GeneratedDocuments_CreatedAt ON dbo.LetterGen_GeneratedDocuments(CreatedAt);
    CREATE INDEX IX_LG_GeneratedDocuments_ClaimNumber ON dbo.LetterGen_GeneratedDocuments(ClaimNumber);
END
ELSE
BEGIN
    PRINT 'Table dbo.LetterGen_GeneratedDocuments already exists.';
END
