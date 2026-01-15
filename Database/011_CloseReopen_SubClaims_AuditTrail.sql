-- =============================================
-- Migration Script: Close/Reopen Sub-Claims with Audit Trail
-- This script adds the SubClaimAudits table and new columns to SubClaims
-- Run this script against your ClaimsPortal database
-- =============================================

USE ClaimsPortal;
GO

PRINT '=== Starting Migration: Close/Reopen Sub-Claims Module ===';
PRINT '';
GO

-- =============================================
-- SECTION 1: Add CloseReason and ReopenReason to SubClaims table
-- =============================================

-- Add CloseReason column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'CloseReason')
BEGIN
    ALTER TABLE SubClaims ADD CloseReason NVARCHAR(100) NULL;
    PRINT 'Added CloseReason to SubClaims table';
END
ELSE
BEGIN
    PRINT 'CloseReason already exists in SubClaims table';
END
GO

-- Add ReopenReason column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ReopenReason')
BEGIN
    ALTER TABLE SubClaims ADD ReopenReason NVARCHAR(100) NULL;
    PRINT 'Added ReopenReason to SubClaims table';
END
ELSE
BEGIN
    PRINT 'ReopenReason already exists in SubClaims table';
END
GO

-- =============================================
-- SECTION 2: Create SubClaimAudits table for audit trail
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SubClaimAudits')
BEGIN
    CREATE TABLE SubClaimAudits (
        SubClaimAuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
        SubClaimId BIGINT NOT NULL,
        Action NVARCHAR(50) NOT NULL,                    -- 'Close' or 'Reopen'
        Reason NVARCHAR(200) NULL,                       -- Close/Reopen reason
        Remarks NVARCHAR(MAX) NULL,                      -- User remarks
        PreviousExpenseReserve DECIMAL(18,2) NULL,       -- Reserve before action
        PreviousIndemnityReserve DECIMAL(18,2) NULL,     -- Reserve before action
        NewExpenseReserve DECIMAL(18,2) NULL,            -- Reserve after action
        NewIndemnityReserve DECIMAL(18,2) NULL,          -- Reserve after action
        PerformedBy NVARCHAR(100) NULL,                  -- User who performed the action
        AuditDate DATETIME NOT NULL DEFAULT GETDATE(),
        
        -- Foreign key constraint
        CONSTRAINT FK_SubClaimAudits_SubClaims 
            FOREIGN KEY (SubClaimId) REFERENCES SubClaims(SubClaimId)
    );
    
    -- Create index for faster lookups
    CREATE NONCLUSTERED INDEX IX_SubClaimAudits_SubClaimId 
        ON SubClaimAudits(SubClaimId);
    
    CREATE NONCLUSTERED INDEX IX_SubClaimAudits_AuditDate 
        ON SubClaimAudits(AuditDate DESC);
    
    PRINT 'Created SubClaimAudits table with indexes';
END
ELSE
BEGIN
    PRINT 'SubClaimAudits table already exists';
END
GO

-- =============================================
-- SECTION 3: Add lookup codes for Close and Reopen reasons
-- =============================================

-- Close Reasons
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'CloseReason' AND RecordCode = 'BELOW_DEDUCTIBLE')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('CloseReason', 'BELOW_DEDUCTIBLE', 'Below Deductible', 'Y', 1, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'CloseReason' AND RecordCode = 'OPEN_IN_ERROR')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('CloseReason', 'OPEN_IN_ERROR', 'Open in Error', 'Y', 2, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'CloseReason' AND RecordCode = 'CLOSE_WITH_NEGOTIATION')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('CloseReason', 'CLOSE_WITH_NEGOTIATION', 'Close with Negotiation', 'Y', 3, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'CloseReason' AND RecordCode = 'CLOSE_WITH_PAYMENT')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('CloseReason', 'CLOSE_WITH_PAYMENT', 'Close with Payment', 'Y', 4, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'CloseReason' AND RecordCode = 'COVERAGE_DENIAL')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('CloseReason', 'COVERAGE_DENIAL', 'Coverage Denial', 'Y', 5, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'CloseReason' AND RecordCode = 'INSURED_REQUEST')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('CloseReason', 'INSURED_REQUEST', 'Insured Request', 'Y', 6, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'CloseReason' AND RecordCode = 'LACK_OF_INTEREST')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('CloseReason', 'LACK_OF_INTEREST', 'Lack of Interest', 'Y', 7, GETDATE(), 'system');
END

PRINT 'Added Close Reason lookup codes';

-- Reopen Reasons
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'ReopenReason' AND RecordCode = 'ADDITIONAL_PAYMENT')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('ReopenReason', 'ADDITIONAL_PAYMENT', 'Additional Payment Required', 'Y', 1, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'ReopenReason' AND RecordCode = 'CLOSE_IN_ERROR')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('ReopenReason', 'CLOSE_IN_ERROR', 'Close in Error', 'Y', 2, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'ReopenReason' AND RecordCode = 'NEW_CLAIMANT')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('ReopenReason', 'NEW_CLAIMANT', 'New Claimant to be added', 'Y', 3, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'ReopenReason' AND RecordCode = 'SUBROGATION')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('ReopenReason', 'SUBROGATION', 'Subrogation', 'Y', 4, GETDATE(), 'system');
END

IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'ReopenReason' AND RecordCode = 'LITIGATION')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedDate, CreatedBy)
    VALUES ('ReopenReason', 'LITIGATION', 'Litigation', 'Y', 5, GETDATE(), 'system');
END

PRINT 'Added Reopen Reason lookup codes';
GO

-- =============================================
-- SECTION 4: Verify changes
-- =============================================

PRINT '';
PRINT '=== Verification ===';

-- Check SubClaims table columns
SELECT 
    'SubClaims' as TableName,
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SubClaims' 
AND COLUMN_NAME IN ('CloseReason', 'ReopenReason')
ORDER BY COLUMN_NAME;

-- Check SubClaimAudits table
SELECT 
    'SubClaimAudits' as TableName,
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SubClaimAudits'
ORDER BY ORDINAL_POSITION;

-- Check Lookup Codes
SELECT 'CloseReason Lookup Codes' as Category, COUNT(*) as Count
FROM LookupCodes WHERE RecordType = 'CloseReason'
UNION ALL
SELECT 'ReopenReason Lookup Codes' as Category, COUNT(*) as Count
FROM LookupCodes WHERE RecordType = 'ReopenReason';

PRINT '';
PRINT '=== Migration Complete ===';
PRINT '';
PRINT 'Summary of changes:';
PRINT '1. SubClaims.CloseReason (NVARCHAR(100)) - Reason for closing the feature';
PRINT '2. SubClaims.ReopenReason (NVARCHAR(100)) - Reason for reopening the feature';
PRINT '3. SubClaimAudits table - Audit trail for all close/reopen operations';
PRINT '4. Close Reason lookup codes (7 reasons)';
PRINT '5. Reopen Reason lookup codes (5 reasons)';
PRINT '';
PRINT 'Business Rules:';
PRINT '- When a feature is closed, both Expense Reserve and Indemnity Reserve are set to $0.00';
PRINT '- Previous reserves are saved in SubClaimAudits for potential restoration';
PRINT '- When reopening, user can choose: keep at $0, restore previous, or set defaults ($500/$1500)';
PRINT '- When ALL sub-claims are closed, the parent claim status changes to Closed';
PRINT '- When ANY sub-claim is reopened, the parent claim status changes to Reopened/Open';
GO
