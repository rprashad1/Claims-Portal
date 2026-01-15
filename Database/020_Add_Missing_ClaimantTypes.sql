-- ============================================================================
-- ADD MISSING CLAIMANT TYPES TO LOOKUPCODES
-- ============================================================================
-- Purpose: Add additional ClaimantType codes that are used by the application
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Adding Missing Claimant Types to LookupCodes ===';
PRINT '';

-- Add Third Party Passenger (TPP)
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'Claimant' AND RecordCode = 'TPP')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedBy, CreatedDate)
    VALUES ('Claimant', 'TPP', 'Third Party Passenger', 'Y', 7, 'System', GETDATE());
    PRINT 'Added: TPP - Third Party Passenger';
END
ELSE
    PRINT 'Exists: TPP - Third Party Passenger';

-- Add Bicyclist (BIC) - alternate code, keeping BYL as well
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'Claimant' AND RecordCode = 'BIC')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedBy, CreatedDate)
    VALUES ('Claimant', 'BIC', 'Bicyclist', 'Y', 8, 'System', GETDATE());
    PRINT 'Added: BIC - Bicyclist';
END
ELSE
    PRINT 'Exists: BIC - Bicyclist';

-- Add Property Damage Owner (PDO)
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'Claimant' AND RecordCode = 'PDO')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedBy, CreatedDate)
    VALUES ('Claimant', 'PDO', 'Property Damage Owner', 'Y', 9, 'System', GETDATE());
    PRINT 'Added: PDO - Property Damage Owner';
END
ELSE
    PRINT 'Exists: PDO - Property Damage Owner';

-- Add Other (OTH) - catch-all for miscellaneous claimant types
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'Claimant' AND RecordCode = 'OTH')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedBy, CreatedDate)
    VALUES ('Claimant', 'OTH', 'Other', 'Y', 99, 'System', GETDATE());
    PRINT 'Added: OTH - Other';
END
ELSE
    PRINT 'Exists: OTH - Other';

-- Add Third Party Vehicle Owner (TPO) for clarity
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'Claimant' AND RecordCode = 'TPO')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedBy, CreatedDate)
    VALUES ('Claimant', 'TPO', 'Third Party Vehicle Owner', 'Y', 10, 'System', GETDATE());
    PRINT 'Added: TPO - Third Party Vehicle Owner';
END
ELSE
    PRINT 'Exists: TPO - Third Party Vehicle Owner';

-- Add Third Party Driver (TPD) for when driver is different from owner
IF NOT EXISTS (SELECT 1 FROM LookupCodes WHERE RecordType = 'Claimant' AND RecordCode = 'TPD')
BEGIN
    INSERT INTO LookupCodes (RecordType, RecordCode, RecordDescription, RecordStatus, SortOrder, CreatedBy, CreatedDate)
    VALUES ('Claimant', 'TPD', 'Third Party Driver', 'Y', 11, 'System', GETDATE());
    PRINT 'Added: TPD - Third Party Driver';
END
ELSE
    PRINT 'Exists: TPD - Third Party Driver';

GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== All Claimant Types in LookupCodes ===';

SELECT 
    RecordCode,
    RecordDescription,
    RecordStatus,
    SortOrder
FROM LookupCodes 
WHERE RecordType = 'Claimant' AND RecordStatus = 'Y'
ORDER BY SortOrder, RecordCode;

-- ============================================================================
-- SUMMARY
-- ============================================================================
PRINT '';
PRINT '================================================';
PRINT 'Complete Claimant Type Codes Reference:';
PRINT '================================================';
PRINT 'IVD  = Insured Vehicle Driver';
PRINT 'IVP  = Insured Vehicle Passenger';
PRINT 'OVD  = Other Vehicle Driver (Third Party Vehicle - Injured)';
PRINT 'OVP  = Other Vehicle Passenger';
PRINT 'PED  = Pedestrian';
PRINT 'BYL  = Bicyclist (legacy code)';
PRINT 'BIC  = Bicyclist (new code)';
PRINT 'TPP  = Third Party Passenger';
PRINT 'TPO  = Third Party Vehicle Owner';
PRINT 'TPD  = Third Party Driver';
PRINT 'PDO  = Property Damage Owner';
PRINT 'OTH  = Other (catch-all)';
PRINT '================================================';
GO

-- ============================================================================
-- NORMALIZE LEGACY CLAIMANT CODES (OVD/OVP -> TPD/TPP)
-- ============================================================================
-- Purpose: Normalize legacy "Other Vehicle" codes to canonical "Third Party" codes
-- Notes:
--  - This is idempotent: running multiple times will have no adverse effect.
--  - Test first inside a transaction and inspect results before committing.
-- ============================================================================

PRINT '';
PRINT '=== Normalizing legacy claimant codes (OVD/OVP -> TPD/TPP) ===';
PRINT '';

BEGIN TRANSACTION;

PRINT 'Preview changes in Claimants table (before)';
SELECT ClaimantId, FnolId, ClaimNumber, ClaimantName, ClaimantType
FROM Claimants
WHERE ClaimantType IN ('OVD','OVP');

-- Update Claimants
UPDATE Claimants
SET ClaimantType = CASE
    WHEN ClaimantType = 'OVD' THEN 'TPD'
    WHEN ClaimantType = 'OVP' THEN 'TPP'
    ELSE ClaimantType END
WHERE ClaimantType IN ('OVD','OVP');

-- Update SubClaims if they reference legacy codes
PRINT 'Preview changes in SubClaims table (before)';
SELECT SubClaimId, FnolId, ClaimNumber, ClaimantType, ClaimantName
FROM SubClaims
WHERE ClaimantType IN ('OVD','OVP');

UPDATE SubClaims
SET ClaimantType = CASE
    WHEN ClaimantType = 'OVD' THEN 'TPD'
    WHEN ClaimantType = 'OVP' THEN 'TPP'
    ELSE ClaimantType END
WHERE ClaimantType IN ('OVD','OVP');

-- Optionally, keep a record of the migration in a simple audit table (create if missing)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MigrationAudit')
BEGIN
    CREATE TABLE MigrationAudit (
        MigrationId INT IDENTITY(1,1) PRIMARY KEY,
        ScriptName NVARCHAR(255),
        Details NVARCHAR(4000),
        ExecutedBy NVARCHAR(100),
        ExecutedAt DATETIME DEFAULT GETDATE()
    );
END

INSERT INTO MigrationAudit (ScriptName, Details, ExecutedBy)
VALUES ('020_Add_Missing_ClaimantTypes.sql', 'Normalized claim types OVD->TPD, OVP->TPP in Claimants and SubClaims', SUSER_SNAME());

PRINT 'Preview changes in Claimants table (after)';
SELECT ClaimantId, FnolId, ClaimNumber, ClaimantName, ClaimantType
FROM Claimants
WHERE ClaimantType IN ('TPD','TPP')
ORDER BY ClaimantType, ClaimantId;

PRINT '';
PRINT 'If the changes look correct run: COMMIT TRANSACTION;'
PRINT 'To roll back: ROLLBACK TRANSACTION;'

-- End of normalization section
GO
