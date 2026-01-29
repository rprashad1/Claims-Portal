-- Create table for storing individual form field values for generated letters
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LetterGen_FormData]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LetterGen_FormData]
    (
        [Id] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        [GeneratedDocumentId] UNIQUEIDENTIFIER NULL,
        [ClaimNumber] NVARCHAR(50) NOT NULL,
        [FieldName] NVARCHAR(200) NOT NULL,
        [FieldValue] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
        [CreatedBy] NVARCHAR(100) NULL
    );

    CREATE INDEX IX_LetterGen_FormData_Claim_Field ON [dbo].[LetterGen_FormData](ClaimNumber, FieldName);
END

const html = `PASTE_THE_RENDER_RESPONSE_HERE`;
window.letterEditor.setIframeSrcdoc(document.getElementById('editableLetterFrame'), html, 'http://localhost:5041/templates/');
