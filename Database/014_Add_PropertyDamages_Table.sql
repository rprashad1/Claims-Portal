-- ============================================================================
-- ADD FNOL PROPERTY DAMAGES TABLE
-- ============================================================================
-- Purpose: Store property damage information linked to FNOL/Claims
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Creating FnolPropertyDamages Table ===';
PRINT '';

-- Create FnolPropertyDamages table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FnolPropertyDamages')
BEGIN
    CREATE TABLE FnolPropertyDamages (
        FnolPropertyDamageId BIGINT IDENTITY(1,1) PRIMARY KEY,
        FnolId BIGINT NOT NULL,
        ClaimNumber NVARCHAR(50) NULL,
        PropertyType NVARCHAR(50) NULL,           -- Building, Fence, Other
        PropertyDescription NVARCHAR(500) NULL,    -- Property description
        
        -- Property Owner Information
        OwnerName NVARCHAR(200) NULL,
        OwnerEntityId BIGINT NULL,                 -- FK to EntityMaster
        OwnerPhone NVARCHAR(20) NULL,
        OwnerEmail NVARCHAR(100) NULL,
        OwnerAddress NVARCHAR(200) NULL,
        OwnerAddress2 NVARCHAR(100) NULL,
        OwnerCity NVARCHAR(100) NULL,
        OwnerState NVARCHAR(2) NULL,
        OwnerZipCode NVARCHAR(10) NULL,
        
        -- Property Location (can be different from owner address)
        PropertyLocation NVARCHAR(500) NULL,
        PropertyAddress NVARCHAR(200) NULL,
        PropertyCity NVARCHAR(100) NULL,
        PropertyState NVARCHAR(2) NULL,
        PropertyZipCode NVARCHAR(10) NULL,
        
        DamageDescription NVARCHAR(MAX) NULL,
        EstimatedDamage DECIMAL(18,2) NULL,
        
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME NULL,
        ModifiedBy NVARCHAR(100) NULL,
        
        -- Foreign key to FNOL
        CONSTRAINT FK_FnolPropertyDamages_FNOL FOREIGN KEY (FnolId) 
            REFERENCES FNOL(FnolId) ON DELETE CASCADE
    );
    
    PRINT 'Created table: FnolPropertyDamages';
    
    -- Create index on FnolId for faster lookups
    CREATE NONCLUSTERED INDEX IX_FnolPropertyDamages_FnolId 
        ON FnolPropertyDamages(FnolId);
    PRINT 'Created index: IX_FnolPropertyDamages_FnolId';
END
ELSE
BEGIN
    PRINT 'Table FnolPropertyDamages already exists';
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== Verification: FnolPropertyDamages Table ===';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FnolPropertyDamages'
ORDER BY ORDINAL_POSITION;

-- ============================================================================
-- SUMMARY
-- ============================================================================
PRINT '';
PRINT '================================================';
PRINT 'FnolPropertyDamages table created successfully';
PRINT '================================================';
PRINT 'This table stores property damage information';
PRINT 'linked to FNOL/Claims, including:';
PRINT '  - Property type and description';
PRINT '  - Owner contact information';
PRINT '  - Property location details';
PRINT '  - Damage description and estimate';
PRINT '================================================';
GO
