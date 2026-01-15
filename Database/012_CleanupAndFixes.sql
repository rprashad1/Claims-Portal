-- =============================================
-- Migration Script: Cleanup and Fixes
-- 1. Add ThirdParty Insurance fields to Vehicles table
-- 2. Remove redundant columns from Claimants table
-- 3. These columns are redundant because they belong in SubClaims table
-- =============================================

USE ClaimsPortal;
GO

PRINT '=== Starting Migration: Cleanup and Fixes ===';
PRINT '';
GO

-- =============================================
-- SECTION 1: Add Insurance fields to Vehicles table for Third Party Vehicles
-- =============================================

-- Add InsuranceCarrier column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Vehicles' AND COLUMN_NAME = 'InsuranceCarrier')
BEGIN
    ALTER TABLE Vehicles ADD InsuranceCarrier NVARCHAR(200) NULL;
    PRINT 'Added InsuranceCarrier to Vehicles table';
END
ELSE
BEGIN
    PRINT 'InsuranceCarrier already exists in Vehicles table';
END
GO

-- Add InsurancePolicyNumber column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Vehicles' AND COLUMN_NAME = 'InsurancePolicyNumber')
BEGIN
    ALTER TABLE Vehicles ADD InsurancePolicyNumber NVARCHAR(50) NULL;
    PRINT 'Added InsurancePolicyNumber to Vehicles table';
END
ELSE
BEGIN
    PRINT 'InsurancePolicyNumber already exists in Vehicles table';
END
GO

-- =============================================
-- SECTION 2: Remove redundant columns from Claimants table
-- These columns belong in SubClaims table, not Claimants
-- SubClaimId, FeatureNumber, Coverage, CoverageDescription, Deductible, CoverageLimits
-- =============================================

PRINT '';
PRINT '=== Removing redundant columns from Claimants table ===';
PRINT '(These columns belong in SubClaims table, not Claimants)';
PRINT '';

-- First, check if any data exists in these columns that we need to preserve
DECLARE @hasData INT = 0;

SELECT @hasData = COUNT(*) FROM Claimants 
WHERE SubClaimId IS NOT NULL 
   OR FeatureNumber IS NOT NULL 
   OR Coverage IS NOT NULL 
   OR CoverageDescription IS NOT NULL 
   OR Deductible IS NOT NULL 
   OR CoverageLimits IS NOT NULL;

IF @hasData > 0
BEGIN
    PRINT 'WARNING: Found ' + CAST(@hasData AS NVARCHAR(10)) + ' rows with data in redundant columns.';
    PRINT 'Data will be lost. The data should already exist in SubClaims table.';
    PRINT '';
END

-- Drop SubClaimId column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'SubClaimId')
BEGIN
    -- First drop any foreign key constraint
    DECLARE @fkName NVARCHAR(200);
    SELECT @fkName = CONSTRAINT_NAME 
    FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE 
    WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'SubClaimId';
    
    IF @fkName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE Claimants DROP CONSTRAINT ' + @fkName);
        PRINT 'Dropped foreign key constraint: ' + @fkName;
    END
    
    ALTER TABLE Claimants DROP COLUMN SubClaimId;
    PRINT 'Dropped SubClaimId from Claimants table';
END
ELSE
BEGIN
    PRINT 'SubClaimId does not exist in Claimants table';
END
GO

-- Drop FeatureNumber column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'FeatureNumber')
BEGIN
    ALTER TABLE Claimants DROP COLUMN FeatureNumber;
    PRINT 'Dropped FeatureNumber from Claimants table';
END
ELSE
BEGIN
    PRINT 'FeatureNumber does not exist in Claimants table';
END
GO

-- Drop Coverage column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'Coverage')
BEGIN
    ALTER TABLE Claimants DROP COLUMN Coverage;
    PRINT 'Dropped Coverage from Claimants table';
END
ELSE
BEGIN
    PRINT 'Coverage does not exist in Claimants table';
END
GO

-- Drop CoverageDescription column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'CoverageDescription')
BEGIN
    ALTER TABLE Claimants DROP COLUMN CoverageDescription;
    PRINT 'Dropped CoverageDescription from Claimants table';
END
ELSE
BEGIN
    PRINT 'CoverageDescription does not exist in Claimants table';
END
GO

-- Drop Deductible column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'Deductible')
BEGIN
    ALTER TABLE Claimants DROP COLUMN Deductible;
    PRINT 'Dropped Deductible from Claimants table';
END
ELSE
BEGIN
    PRINT 'Deductible does not exist in Claimants table';
END
GO

-- Drop CoverageLimits column
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'CoverageLimits')
BEGIN
    ALTER TABLE Claimants DROP COLUMN CoverageLimits;
    PRINT 'Dropped CoverageLimits from Claimants table';
END
ELSE
BEGIN
    PRINT 'CoverageLimits does not exist in Claimants table';
END
GO

-- =============================================
-- SECTION 3: Verification
-- =============================================

PRINT '';
PRINT '=== Verification ===';

-- Check Vehicles table for new columns
PRINT '';
PRINT 'Vehicles table - Insurance columns:';
SELECT 
    'Vehicles' as TableName,
    COLUMN_NAME, 
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Vehicles' 
AND COLUMN_NAME IN ('InsuranceCarrier', 'InsurancePolicyNumber')
ORDER BY COLUMN_NAME;

-- Check Claimants table - verify columns removed
PRINT '';
PRINT 'Claimants table - Current columns:';
SELECT 
    'Claimants' as TableName,
    COLUMN_NAME, 
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Claimants'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '=== Migration Complete ===';
PRINT '';
PRINT 'Summary:';
PRINT '1. Added InsuranceCarrier (NVARCHAR(200)) to Vehicles table for Third Party insurance info';
PRINT '2. Added InsurancePolicyNumber (NVARCHAR(50)) to Vehicles table for Third Party policy number';
PRINT '3. Removed redundant columns from Claimants table:';
PRINT '   - SubClaimId (data belongs in SubClaims table with ClaimantEntityId)';
PRINT '   - FeatureNumber (belongs in SubClaims table)';
PRINT '   - Coverage (belongs in SubClaims table)';
PRINT '   - CoverageDescription (belongs in SubClaims table)';
PRINT '   - Deductible (belongs in SubClaims table)';
PRINT '   - CoverageLimits (belongs in SubClaims table)';
PRINT '';
PRINT 'IMPORTANT: The relationship between Claimants and SubClaims is via ClaimantEntityId';
PRINT '           SubClaims.ClaimantEntityId -> EntityMaster.EntityId <- Claimants.ClaimantEntityId';
GO
