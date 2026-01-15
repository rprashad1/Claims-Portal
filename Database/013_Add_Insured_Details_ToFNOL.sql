-- ============================================================================
-- ADD COMPLETE INSURED DETAILS COLUMNS TO FNOL TABLE
-- ============================================================================
-- Purpose: Store full insured party information including address and contact
-- so that the claim can display complete insured details on the view page.
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Adding Insured Details Columns to FNOL Table ===';
PRINT '';

-- Add InsuredEmail column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredEmail')
BEGIN
    ALTER TABLE FNOL ADD InsuredEmail NVARCHAR(100) NULL;
    PRINT 'Added column: InsuredEmail';
END
ELSE
BEGIN
    PRINT 'Column InsuredEmail already exists';
END
GO

-- Add InsuredDoingBusinessAs column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredDoingBusinessAs')
BEGIN
    ALTER TABLE FNOL ADD InsuredDoingBusinessAs NVARCHAR(200) NULL;
    PRINT 'Added column: InsuredDoingBusinessAs';
END
ELSE
BEGIN
    PRINT 'Column InsuredDoingBusinessAs already exists';
END
GO

-- Add InsuredBusinessType column (Individual or Business)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredBusinessType')
BEGIN
    ALTER TABLE FNOL ADD InsuredBusinessType NVARCHAR(50) NULL;
    PRINT 'Added column: InsuredBusinessType';
END
ELSE
BEGIN
    PRINT 'Column InsuredBusinessType already exists';
END
GO

-- Add InsuredAddress column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredAddress')
BEGIN
    ALTER TABLE FNOL ADD InsuredAddress NVARCHAR(200) NULL;
    PRINT 'Added column: InsuredAddress';
END
ELSE
BEGIN
    PRINT 'Column InsuredAddress already exists';
END
GO

-- Add InsuredAddress2 column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredAddress2')
BEGIN
    ALTER TABLE FNOL ADD InsuredAddress2 NVARCHAR(100) NULL;
    PRINT 'Added column: InsuredAddress2';
END
ELSE
BEGIN
    PRINT 'Column InsuredAddress2 already exists';
END
GO

-- Add InsuredCity column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredCity')
BEGIN
    ALTER TABLE FNOL ADD InsuredCity NVARCHAR(100) NULL;
    PRINT 'Added column: InsuredCity';
END
ELSE
BEGIN
    PRINT 'Column InsuredCity already exists';
END
GO

-- Add InsuredState column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredState')
BEGIN
    ALTER TABLE FNOL ADD InsuredState NVARCHAR(2) NULL;
    PRINT 'Added column: InsuredState';
END
ELSE
BEGIN
    PRINT 'Column InsuredState already exists';
END
GO

-- Add InsuredZipCode column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredZipCode')
BEGIN
    ALTER TABLE FNOL ADD InsuredZipCode NVARCHAR(10) NULL;
    PRINT 'Added column: InsuredZipCode';
END
ELSE
BEGIN
    PRINT 'Column InsuredZipCode already exists';
END
GO

-- Add InsuredFeinSsNumber column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredFeinSsNumber')
BEGIN
    ALTER TABLE FNOL ADD InsuredFeinSsNumber NVARCHAR(50) NULL;
    PRINT 'Added column: InsuredFeinSsNumber';
END
ELSE
BEGIN
    PRINT 'Column InsuredFeinSsNumber already exists';
END
GO

-- Add InsuredLicenseNumber column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredLicenseNumber')
BEGIN
    ALTER TABLE FNOL ADD InsuredLicenseNumber NVARCHAR(50) NULL;
    PRINT 'Added column: InsuredLicenseNumber';
END
ELSE
BEGIN
    PRINT 'Column InsuredLicenseNumber already exists';
END
GO

-- Add InsuredLicenseState column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredLicenseState')
BEGIN
    ALTER TABLE FNOL ADD InsuredLicenseState NVARCHAR(2) NULL;
    PRINT 'Added column: InsuredLicenseState';
END
ELSE
BEGIN
    PRINT 'Column InsuredLicenseState already exists';
END
GO

-- Add InsuredDateOfBirth column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FNOL') AND name = 'InsuredDateOfBirth')
BEGIN
    ALTER TABLE FNOL ADD InsuredDateOfBirth DATE NULL;
    PRINT 'Added column: InsuredDateOfBirth';
END
ELSE
BEGIN
    PRINT 'Column InsuredDateOfBirth already exists';
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== Verification: FNOL Insured Columns ===';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FNOL' 
AND COLUMN_NAME LIKE 'Insured%'
ORDER BY COLUMN_NAME;

-- ============================================================================
-- SUMMARY
-- ============================================================================
PRINT '';
PRINT '============================================';
PRINT 'Insured Details columns added to FNOL table';
PRINT '============================================';
PRINT 'New columns:';
PRINT '  - InsuredEmail (NVARCHAR 100)';
PRINT '  - InsuredDoingBusinessAs (NVARCHAR 200)';
PRINT '  - InsuredBusinessType (NVARCHAR 50)';
PRINT '  - InsuredAddress (NVARCHAR 200)';
PRINT '  - InsuredAddress2 (NVARCHAR 100)';
PRINT '  - InsuredCity (NVARCHAR 100)';
PRINT '  - InsuredState (NVARCHAR 2)';
PRINT '  - InsuredZipCode (NVARCHAR 10)';
PRINT '  - InsuredFeinSsNumber (NVARCHAR 50)';
PRINT '  - InsuredLicenseNumber (NVARCHAR 50)';
PRINT '  - InsuredLicenseState (NVARCHAR 2)';
PRINT '  - InsuredDateOfBirth (DATE)';
PRINT '';
PRINT 'These columns store complete insured party';
PRINT 'information when policy is verified during';
PRINT 'FNOL creation, enabling full display on the';
PRINT 'Claim Detail view page.';
PRINT '============================================';
GO
