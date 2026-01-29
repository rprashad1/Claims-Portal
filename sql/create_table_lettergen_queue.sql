-- Create durable queue table for letter generation jobs
CREATE TABLE [dbo].[LetterGen_Queue]
(
    QueueId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ClaimNumber NVARCHAR(50) NOT NULL,
    SelectedRuleIds NVARCHAR(MAX) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    Tries INT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    LastAttemptAt DATETIMEOFFSET NULL,
    LastError NVARCHAR(MAX) NULL,
    ProcessingHostname NVARCHAR(255) NULL
);

CREATE INDEX IX_LetterGen_Queue_Status_CreatedAt ON [dbo].[LetterGen_Queue](Status, CreatedAt);
