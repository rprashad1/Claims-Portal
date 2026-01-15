-- ============================================================================
-- ADD INJURED PARTY FIELD TO CLAIMANTS TABLE
-- ============================================================================
-- Purpose: Add InjuredParty field to identify who is the injured party
-- for third-party vehicle claims where owner and driver are different
-- Values: "Owner" or "Driver"
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Adding InjuredParty Field to Claimants ===';
PRINT '';

-- Add InjuredParty column
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'InjuredParty')
BEGIN
    ALTER TABLE Claimants ADD InjuredParty NVARCHAR(20) NULL;
    PRINT 'Added InjuredParty column';
    
    -- Add comment describing the field
    EXEC sys.sp_addextendedproperty 
        @name = N'MS_Description', 
        @value = N'For third-party vehicle claims: Identifies who is the injured party and claimant. Values: "Owner" or "Driver". When driver is different from owner, this field indicates which person is the claimant.', 
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'Claimants',
        @level2type = N'COLUMN', @level2name = N'InjuredParty';
END
ELSE
BEGIN
    PRINT 'InjuredParty column already exists';
END
GO

-- ============================================================================
-- Also add to ThirdParties table if it exists (for denormalized storage)
-- ============================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ThirdParties')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'ThirdParties' AND COLUMN_NAME = 'InjuredParty')
    BEGIN
        ALTER TABLE ThirdParties ADD InjuredParty NVARCHAR(20) NULL;
        PRINT 'Added InjuredParty column to ThirdParties table';
    END
    ELSE
    BEGIN
        PRINT 'InjuredParty column already exists in ThirdParties table';
    END
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== Verification: Claimants Table InjuredParty Field ===';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Claimants'
  AND COLUMN_NAME = 'InjuredParty';
GO

PRINT '';
PRINT '================================================';
PRINT 'InjuredParty Field Added Successfully';
PRINT '================================================';
PRINT '';
PRINT 'Claimants.InjuredParty column:';
PRINT '  - Purpose: Identifies the injured party in third-party vehicle claims';
PRINT '  - Values: "Owner" or "Driver"';
PRINT '  - Usage: When ThirdParty.Type = "Vehicle" and owner != driver,';
PRINT '           this field determines who is the claimant';
PRINT '';
PRINT 'UI Impact:';
PRINT '  - Claimant badge appears on the correct person';
PRINT '  - Injury details shown under the claimant section';
PRINT '  - Attorney info shown under the claimant section';
PRINT '  - Hospital info shown under the claimant section';
PRINT '================================================';
GO
