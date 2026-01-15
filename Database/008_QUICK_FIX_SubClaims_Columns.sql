-- =============================================
-- QUICK FIX: Add missing columns to SubClaims table
-- Run this script against your ClaimsPortal database
-- =============================================

USE ClaimsPortal;
GO

-- Add ClaimantEntityId
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ClaimantEntityId')
BEGIN
    ALTER TABLE SubClaims ADD ClaimantEntityId BIGINT NULL;
    PRINT 'Added ClaimantEntityId';
END
GO

-- Add AssignedAdjusterName
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'AssignedAdjusterName')
BEGIN
    ALTER TABLE SubClaims ADD AssignedAdjusterName NVARCHAR(200) NULL;
    PRINT 'Added AssignedAdjusterName';
END
GO

-- Add ClosedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ClosedBy')
BEGIN
    ALTER TABLE SubClaims ADD ClosedBy NVARCHAR(100) NULL;
    PRINT 'Added ClosedBy';
END
GO

-- Add ReopenedDate
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ReopenedDate')
BEGIN
    ALTER TABLE SubClaims ADD ReopenedDate DATETIME NULL;
    PRINT 'Added ReopenedDate';
END
GO

-- Add ReopenedBy
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ReopenedBy')
BEGIN
    ALTER TABLE SubClaims ADD ReopenedBy NVARCHAR(100) NULL;
    PRINT 'Added ReopenedBy';
END
GO

PRINT '';
PRINT '=== SubClaims columns added successfully ===';
PRINT 'You can now save FNOLs with the new EntityMaster linking.';
GO
