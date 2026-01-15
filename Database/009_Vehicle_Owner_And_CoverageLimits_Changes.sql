-- =============================================
-- Migration Script: Add VehicleOwnerEntityId and DriverEntityId to Vehicles
--                   Change CoverageLimits from DECIMAL to NVARCHAR
-- Run this script against your ClaimsPortal database
-- =============================================

USE ClaimsPortal;
GO

PRINT '=== Starting Migration: Vehicle Entity IDs and CoverageLimits Type Change ===';
PRINT '';
GO

-- =============================================
-- SECTION 1: Add Entity ID columns to Vehicles table
-- =============================================

-- Add VehicleOwnerEntityId to Vehicles
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Vehicles' AND COLUMN_NAME = 'VehicleOwnerEntityId')
BEGIN
    ALTER TABLE Vehicles ADD VehicleOwnerEntityId BIGINT NULL;
    PRINT 'Added VehicleOwnerEntityId to Vehicles table';
END
ELSE
BEGIN
    PRINT 'VehicleOwnerEntityId already exists in Vehicles table';
END
GO

-- Add DriverEntityId to Vehicles (for third party vehicles where driver may differ from owner)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Vehicles' AND COLUMN_NAME = 'DriverEntityId')
BEGIN
    ALTER TABLE Vehicles ADD DriverEntityId BIGINT NULL;
    PRINT 'Added DriverEntityId to Vehicles table';
END
ELSE
BEGIN
    PRINT 'DriverEntityId already exists in Vehicles table';
END
GO

-- Add PlateState if not exists (some scripts may have missed it)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Vehicles' AND COLUMN_NAME = 'PlateState')
BEGIN
    ALTER TABLE Vehicles ADD PlateState NVARCHAR(10) NULL;
    PRINT 'Added PlateState to Vehicles table';
END
GO

-- =============================================
-- SECTION 2: Change CoverageLimits in SubClaims from DECIMAL to NVARCHAR
-- This supports values like "100,000/300,000" or "25,000/50,000"
-- =============================================

-- First check if column is already NVARCHAR
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'SubClaims' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE IN ('nvarchar', 'varchar'))
BEGIN
    PRINT 'SubClaims.CoverageLimits is already NVARCHAR type - no change needed';
END
GO

-- Check if column doesn't exist at all
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'CoverageLimits')
BEGIN
    ALTER TABLE SubClaims ADD CoverageLimits NVARCHAR(100) NULL;
    PRINT 'Added CoverageLimits as NVARCHAR(100) to SubClaims table';
END
GO

-- Handle DECIMAL to NVARCHAR conversion for SubClaims
-- Step 2a: Add temporary column if conversion is needed
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'SubClaims' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE = 'decimal')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'SubClaims' 
                   AND COLUMN_NAME = 'CoverageLimits_New')
BEGIN
    ALTER TABLE SubClaims ADD CoverageLimits_New NVARCHAR(100) NULL;
    PRINT 'Added temporary CoverageLimits_New column to SubClaims';
END
GO

-- Step 2b: Copy data
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'SubClaims' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE = 'decimal')
   AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' 
               AND COLUMN_NAME = 'CoverageLimits_New')
BEGIN
    UPDATE SubClaims 
    SET CoverageLimits_New = CASE 
        WHEN CoverageLimits IS NULL THEN NULL
        WHEN CoverageLimits = 0 THEN NULL
        ELSE FORMAT(CoverageLimits, 'N0')
    END;
    PRINT 'Migrated existing CoverageLimits data to new format';
END
GO

-- Step 2c: Drop old column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'SubClaims' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE = 'decimal')
BEGIN
    ALTER TABLE SubClaims DROP COLUMN CoverageLimits;
    PRINT 'Dropped old CoverageLimits column from SubClaims';
END
GO

-- Step 2d: Rename new column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'SubClaims' 
           AND COLUMN_NAME = 'CoverageLimits_New')
BEGIN
    EXEC sp_rename 'SubClaims.CoverageLimits_New', 'CoverageLimits', 'COLUMN';
    PRINT 'Renamed CoverageLimits_New to CoverageLimits in SubClaims';
END
GO

-- =============================================
-- SECTION 3: Change CoverageLimits in Claimants from DECIMAL to NVARCHAR
-- =============================================

-- First check if column is already NVARCHAR
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE IN ('nvarchar', 'varchar'))
BEGIN
    PRINT 'Claimants.CoverageLimits is already NVARCHAR type - no change needed';
END
GO

-- Handle DECIMAL to NVARCHAR conversion for Claimants
-- Step 3a: Add temporary column if conversion is needed
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE = 'decimal')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Claimants' 
                   AND COLUMN_NAME = 'CoverageLimits_New')
BEGIN
    ALTER TABLE Claimants ADD CoverageLimits_New NVARCHAR(100) NULL;
    PRINT 'Added temporary CoverageLimits_New column to Claimants';
END
GO

-- Step 3b: Copy data
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE = 'decimal')
   AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' 
               AND COLUMN_NAME = 'CoverageLimits_New')
BEGIN
    UPDATE Claimants 
    SET CoverageLimits_New = CASE 
        WHEN CoverageLimits IS NULL THEN NULL
        WHEN CoverageLimits = 0 THEN NULL
        ELSE FORMAT(CoverageLimits, 'N0')
    END;
    PRINT 'Migrated existing Claimants.CoverageLimits data to new format';
END
GO

-- Step 3c: Drop old column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' 
           AND COLUMN_NAME = 'CoverageLimits' 
           AND DATA_TYPE = 'decimal')
BEGIN
    ALTER TABLE Claimants DROP COLUMN CoverageLimits;
    PRINT 'Dropped old CoverageLimits column from Claimants';
END
GO

-- Step 3d: Rename new column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' 
           AND COLUMN_NAME = 'CoverageLimits_New')
BEGIN
    EXEC sp_rename 'Claimants.CoverageLimits_New', 'CoverageLimits', 'COLUMN';
    PRINT 'Renamed CoverageLimits_New to CoverageLimits in Claimants';
END
GO

-- =============================================
-- SECTION 4: Verify changes
-- =============================================

PRINT '';
PRINT '=== Verification ===';

-- Check Vehicles table columns
SELECT 
    'Vehicles' as TableName,
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Vehicles' 
AND COLUMN_NAME IN ('VehicleOwnerEntityId', 'DriverEntityId', 'PlateState')
ORDER BY COLUMN_NAME;

-- Check SubClaims.CoverageLimits
SELECT 
    'SubClaims' as TableName,
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SubClaims' 
AND COLUMN_NAME = 'CoverageLimits';

-- Check Claimants.CoverageLimits
SELECT 
    'Claimants' as TableName,
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Claimants' 
AND COLUMN_NAME = 'CoverageLimits';

PRINT '';
PRINT '=== Migration Complete ===';
PRINT '';
PRINT 'Summary of changes:';
PRINT '1. Vehicles.VehicleOwnerEntityId (BIGINT) - Links to vehicle owner in EntityMaster';
PRINT '2. Vehicles.DriverEntityId (BIGINT) - Links to driver in EntityMaster (if different from owner)';
PRINT '3. SubClaims.CoverageLimits changed from DECIMAL to NVARCHAR(100)';
PRINT '4. Claimants.CoverageLimits changed from DECIMAL to NVARCHAR(100)';
PRINT '';
PRINT 'CoverageLimits now supports values like:';
PRINT '  - "100,000/300,000" (Bodily Injury: $100K per person / $300K per accident)';
PRINT '  - "25,000/50,000" (Lower limit BI coverage)';
PRINT '  - "50,000" (Single limit like PD coverage)';
PRINT '  - "250,000 CSL" (Combined Single Limit)';
GO
