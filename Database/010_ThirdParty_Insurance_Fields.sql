-- =============================================
-- Migration Script: Add Insurance Carrier and Policy Number for Third Party Vehicles
-- Run this script against your ClaimsPortal database
-- =============================================

USE ClaimsPortal;
GO

PRINT '=== Starting Migration: Third Party Insurance Fields ===';
PRINT '';
GO

-- =============================================
-- SECTION 1: Add Insurance fields to Vehicles table
-- These fields capture the third party vehicle owner's insurance information
-- =============================================

-- Add InsuranceCarrier to Vehicles
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

-- Add InsurancePolicyNumber to Vehicles
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Vehicles' AND COLUMN_NAME = 'InsurancePolicyNumber')
BEGIN
    ALTER TABLE Vehicles ADD InsurancePolicyNumber NVARCHAR(100) NULL;
    PRINT 'Added InsurancePolicyNumber to Vehicles table';
END
ELSE
BEGIN
    PRINT 'InsurancePolicyNumber already exists in Vehicles table';
END
GO

-- =============================================
-- SECTION 2: Verify changes
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
AND COLUMN_NAME IN ('InsuranceCarrier', 'InsurancePolicyNumber')
ORDER BY COLUMN_NAME;

PRINT '';
PRINT '=== Migration Complete ===';
PRINT '';
PRINT 'Summary of changes:';
PRINT '1. Vehicles.InsuranceCarrier (NVARCHAR(200)) - Third party insurance carrier name';
PRINT '2. Vehicles.InsurancePolicyNumber (NVARCHAR(100)) - Third party insurance policy number';
PRINT '';
PRINT 'These fields are optional and used to capture the third party vehicle';
PRINT 'owner''s insurance information during FNOL entry.';
GO
