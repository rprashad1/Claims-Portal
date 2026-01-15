-- =============================================
-- Migration Script: 006_SubClaim_Status_Fields.sql
-- Purpose: Add fields to support sub-claim close/reopen business rules
-- Date: January 2025
-- =============================================

USE ClaimsPortal;
GO

-- =============================================
-- Add ClosedBy field to SubClaims table
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ClosedBy')
BEGIN
    ALTER TABLE SubClaims
    ADD ClosedBy NVARCHAR(100) NULL;
    
    PRINT 'Added ClosedBy column to SubClaims table';
END
GO

-- =============================================
-- Add ReopenedBy field to SubClaims table
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ReopenedBy')
BEGIN
    ALTER TABLE SubClaims
    ADD ReopenedBy NVARCHAR(100) NULL;
    
    PRINT 'Added ReopenedBy column to SubClaims table';
END
GO

-- =============================================
-- Add ReopenedDate field to SubClaims table
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ReopenedDate')
BEGIN
    ALTER TABLE SubClaims
    ADD ReopenedDate DATETIME NULL;
    
    PRINT 'Added ReopenedDate column to SubClaims table';
END
GO

-- =============================================
-- Add ModifiedTime field to SubClaims table
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ModifiedTime')
BEGIN
    ALTER TABLE SubClaims
    ADD ModifiedTime TIME NULL;
    
    PRINT 'Added ModifiedTime column to SubClaims table';
END
GO

-- =============================================
-- Add OpenedDate if not exists (ensure it's present)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'OpenedDate')
BEGIN
    ALTER TABLE SubClaims
    ADD OpenedDate DATETIME NULL;
    
    PRINT 'Added OpenedDate column to SubClaims table';
END
GO

-- =============================================
-- Add ClosedDate if not exists (ensure it's present)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ClosedDate')
BEGIN
    ALTER TABLE SubClaims
    ADD ClosedDate DATETIME NULL;
    
    PRINT 'Added ClosedDate column to SubClaims table';
END
GO

-- =============================================
-- Add index on SubClaimStatus for faster queries
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_SubClaims_Status' AND object_id = OBJECT_ID('SubClaims'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubClaims_Status
    ON SubClaims (SubClaimStatus)
    INCLUDE (FnolId, ClaimNumber);
    
    PRINT 'Created index IX_SubClaims_Status on SubClaims table';
END
GO

-- =============================================
-- Add index on FnolId for faster joins
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_SubClaims_FnolId' AND object_id = OBJECT_ID('SubClaims'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubClaims_FnolId
    ON SubClaims (FnolId)
    INCLUDE (SubClaimStatus, ClosedDate);
    
    PRINT 'Created index IX_SubClaims_FnolId on SubClaims table';
END
GO

-- =============================================
-- Verify the changes
-- =============================================
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SubClaims'
AND COLUMN_NAME IN ('ClosedBy', 'ReopenedBy', 'ReopenedDate', 'ModifiedTime', 'OpenedDate', 'ClosedDate', 'SubClaimStatus')
ORDER BY COLUMN_NAME;
GO

-- =============================================
-- Display current SubClaims table structure
-- =============================================
PRINT '';
PRINT '=== SubClaims Table Structure ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COALESCE(CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR), 
             CAST(NUMERIC_PRECISION AS VARCHAR) + ',' + CAST(NUMERIC_SCALE AS VARCHAR), 
             '') AS [Length/Precision]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SubClaims'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT 'Migration 006 complete: Sub-Claim status fields added successfully';
PRINT '';
PRINT 'Business Rules Supported:';
PRINT '  - Sub-claims start with status O (Open)';
PRINT '  - Sub-claims can be closed (status C) with ClosedBy and ClosedDate tracked';
PRINT '  - Sub-claims can be reopened (status R) with ReopenedBy and ReopenedDate tracked';
PRINT '  - Claim status auto-updates based on sub-claim statuses';
GO