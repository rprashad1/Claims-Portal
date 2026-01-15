-- ============================================================================
-- ADD SAMPLE ADJUSTERS TO ENTITYMASTER
-- ============================================================================
-- Purpose: Create sample adjuster records in EntityMaster for feature/sub-claim assignment
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Adding Sample Adjusters ===';
PRINT '';

-- Check if adjusters already exist
IF NOT EXISTS (SELECT 1 FROM EntityMaster WHERE PartyType = 'Adjuster' OR VendorType = 'Adjuster')
BEGIN
    -- Insert sample adjusters
    INSERT INTO EntityMaster (
        EntityType,
        PartyType,
        EntityGroupCode,
        VendorType,
        EntityName,
        HomeBusinessPhone,
        MobilePhone,
        Email,
        EntityStatus,
        CreatedDate,
        CreatedBy
    )
    VALUES
    ('I', 'Adjuster', 'Employee', 'Adjuster', 'Raj Patel', '(555) 100-0001', '(555) 200-0001', 'raj.patel@claimsportal.com', 'Y', GETDATE(), 'System'),
    ('I', 'Adjuster', 'Employee', 'Adjuster', 'Edwin Martinez', '(555) 100-0002', '(555) 200-0002', 'edwin.martinez@claimsportal.com', 'Y', GETDATE(), 'System'),
    ('I', 'Adjuster', 'Employee', 'Adjuster', 'Constantine George', '(555) 100-0003', '(555) 200-0003', 'constantine.george@claimsportal.com', 'Y', GETDATE(), 'System'),
    ('I', 'Adjuster', 'Employee', 'Adjuster', 'Joan Williams', '(555) 100-0004', '(555) 200-0004', 'joan.williams@claimsportal.com', 'Y', GETDATE(), 'System'),
    ('I', 'Adjuster', 'Employee', 'Adjuster', 'Pamela Baldwin', '(555) 100-0005', '(555) 200-0005', 'pamela.baldwin@claimsportal.com', 'Y', GETDATE(), 'System'),
    ('I', 'Adjuster', 'Employee', 'Adjuster', 'Christine Wood', '(555) 100-0006', '(555) 200-0006', 'christine.wood@claimsportal.com', 'Y', GETDATE(), 'System'),
    ('I', 'Adjuster', 'Employee', 'Adjuster', 'Lens Jacques', '(555) 100-0007', '(555) 200-0007', 'lens.jacques@claimsportal.com', 'Y', GETDATE(), 'System');

    PRINT 'Inserted 7 sample adjusters';
END
ELSE
BEGIN
    PRINT 'Adjusters already exist in the system';
END
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== Verification: Adjusters in EntityMaster ===';

SELECT 
    EntityId,
    EntityName,
    PartyType,
    VendorType,
    HomeBusinessPhone,
    Email,
    EntityStatus
FROM EntityMaster 
WHERE PartyType = 'Adjuster' OR VendorType = 'Adjuster'
ORDER BY EntityName;
GO

-- ============================================================================
-- IMPORTANT NOTES
-- ============================================================================
PRINT '';
PRINT '================================================';
PRINT 'Sample Adjusters Added Successfully';
PRINT '================================================';
PRINT '';
PRINT 'These adjusters will appear in the SubClaim modal';
PRINT 'when creating features/sub-claims.';
PRINT '';
PRINT 'The AssignedAdjusterId column in SubClaims table';
PRINT 'stores the EntityId of the selected adjuster.';
PRINT '';
PRINT 'This ensures:';
PRINT '  1. Unique identification even if names are similar';
PRINT '  2. Proper linking to adjuster contact information';
PRINT '  3. Audit trail for assignments';
PRINT '================================================';
GO
