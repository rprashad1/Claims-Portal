-- ============================================================================
-- ADD HOSPITAL INFORMATION TO CLAIMANTS TABLE
-- ============================================================================
-- Purpose: Add hospital name and address fields to Claimants table
-- to store injury hospitalization details
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Adding Hospital Information Fields to Claimants ===';
PRINT '';

-- Add HospitalName column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'HospitalName')
BEGIN
    ALTER TABLE Claimants ADD HospitalName NVARCHAR(200) NULL;
    PRINT 'Added HospitalName column';
END
ELSE
BEGIN
    PRINT 'HospitalName column already exists';
END
GO

-- Add HospitalStreetAddress column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'HospitalStreetAddress')
BEGIN
    ALTER TABLE Claimants ADD HospitalStreetAddress NVARCHAR(500) NULL;
    PRINT 'Added HospitalStreetAddress column';
END
ELSE
BEGIN
    PRINT 'HospitalStreetAddress column already exists';
END
GO

-- Add HospitalCity column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'HospitalCity')
BEGIN
    ALTER TABLE Claimants ADD HospitalCity NVARCHAR(100) NULL;
    PRINT 'Added HospitalCity column';
END
ELSE
BEGIN
    PRINT 'HospitalCity column already exists';
END
GO

-- Add HospitalState column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'HospitalState')
BEGIN
    ALTER TABLE Claimants ADD HospitalState NVARCHAR(50) NULL;
    PRINT 'Added HospitalState column';
END
ELSE
BEGIN
    PRINT 'HospitalState column already exists';
END
GO

-- Add HospitalZipCode column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'HospitalZipCode')
BEGIN
    ALTER TABLE Claimants ADD HospitalZipCode NVARCHAR(20) NULL;
    PRINT 'Added HospitalZipCode column';
END
ELSE
BEGIN
    PRINT 'HospitalZipCode column already exists';
END
GO

-- Add TreatingPhysician column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'TreatingPhysician')
BEGIN
    ALTER TABLE Claimants ADD TreatingPhysician NVARCHAR(200) NULL;
    PRINT 'Added TreatingPhysician column';
END
ELSE
BEGIN
    PRINT 'TreatingPhysician column already exists';
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== Verification: Claimants Table Structure ===';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Claimants'
  AND COLUMN_NAME IN ('HospitalName', 'HospitalStreetAddress', 'HospitalCity', 
                       'HospitalState', 'HospitalZipCode', 'TreatingPhysician')
ORDER BY COLUMN_NAME;
GO

PRINT '';
PRINT '================================================';
PRINT 'Hospital Information Fields Added Successfully';
PRINT '================================================';
PRINT '';
PRINT 'New Claimants columns:';
PRINT '  - HospitalName: Name of the hospital';
PRINT '  - HospitalStreetAddress: Hospital street address';
PRINT '  - HospitalCity: Hospital city';
PRINT '  - HospitalState: Hospital state';
PRINT '  - HospitalZipCode: Hospital ZIP code';
PRINT '  - TreatingPhysician: Name of treating physician';
PRINT '';
PRINT 'These fields store injury hospitalization details';
PRINT 'when IsHospitalized = 1';
PRINT '================================================';
GO
