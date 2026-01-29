-- Adds TemplateFile column to dbo.LetterGen_AdminRules so rules can reference an HTML template
-- Run in the ClaimsPortal database (backup first in production)

IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'dbo' AND t.name = 'LetterGen_AdminRules' AND c.name = 'TemplateFile'
)
BEGIN
    ALTER TABLE dbo.LetterGen_AdminRules
    ADD TemplateFile NVARCHAR(260) NULL;
    PRINT 'Column TemplateFile added to dbo.LetterGen_AdminRules.';
END
ELSE
BEGIN
    PRINT 'Column TemplateFile already exists.';
END
