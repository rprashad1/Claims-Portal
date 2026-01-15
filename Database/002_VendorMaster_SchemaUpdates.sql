-- ============================================================================
-- VENDOR MASTER SCHEMA UPDATES
-- Run this script AFTER 001_InitialSchema.sql
-- ============================================================================

-- ============================================================================
-- CHANGE 1: Add Fax # and Contact Name columns to EntityMaster table
-- ============================================================================

-- Add FaxNumber column to EntityMaster
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EntityMaster') AND name = 'FaxNumber')
BEGIN
    ALTER TABLE EntityMaster
    ADD FaxNumber NVARCHAR(20) NULL;
    
    PRINT 'Added FaxNumber column to EntityMaster table.';
END
ELSE
BEGIN
    PRINT 'FaxNumber column already exists in EntityMaster table.';
END
GO

-- Add ContactName column to EntityMaster
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EntityMaster') AND name = 'ContactName')
BEGIN
    ALTER TABLE EntityMaster
    ADD ContactName NVARCHAR(200) NULL;
    
    PRINT 'Added ContactName column to EntityMaster table.';
END
ELSE
BEGIN
    PRINT 'ContactName column already exists in EntityMaster table.';
END
GO

-- ============================================================================
-- CHANGE 2: Rename Is1099Reportable to W9Received in EntityMaster table
-- ============================================================================

-- Check if old column exists and new column doesn't exist, then rename
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EntityMaster') AND name = 'Is1099Reportable')
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EntityMaster') AND name = 'W9Received')
BEGIN
    EXEC sp_rename 'EntityMaster.Is1099Reportable', 'W9Received', 'COLUMN';
    
    PRINT 'Renamed Is1099Reportable column to W9Received in EntityMaster table.';
END
ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EntityMaster') AND name = 'W9Received')
BEGIN
    PRINT 'W9Received column already exists in EntityMaster table.';
END
ELSE
BEGIN
    -- If Is1099Reportable doesn't exist, add W9Received as new column
    ALTER TABLE EntityMaster
    ADD W9Received BIT DEFAULT 0;
    
    PRINT 'Added W9Received column to EntityMaster table (Is1099Reportable did not exist).';
END
GO

-- ============================================================================
-- CHANGE 3: Add Fax # column to AddressMaster table
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AddressMaster') AND name = 'FaxNumber')
BEGIN
    ALTER TABLE AddressMaster
    ADD FaxNumber NVARCHAR(20) NULL;
    
    PRINT 'Added FaxNumber column to AddressMaster table.';
END
ELSE
BEGIN
    PRINT 'FaxNumber column already exists in AddressMaster table.';
END
GO

-- ============================================================================
-- VERIFICATION: Display updated table structures
-- ============================================================================

PRINT '';
PRINT '============================================';
PRINT 'VERIFICATION: EntityMaster columns';
PRINT '============================================';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'EntityMaster'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '============================================';
PRINT 'VERIFICATION: AddressMaster columns';
PRINT '============================================';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AddressMaster'
ORDER BY ORDINAL_POSITION;

GO

PRINT '';
PRINT '============================================';
PRINT 'ALL SCHEMA UPDATES COMPLETED SUCCESSFULLY!';
PRINT '============================================';
