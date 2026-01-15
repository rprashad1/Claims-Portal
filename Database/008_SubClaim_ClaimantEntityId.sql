-- =============================================
-- Migration Script: 008_SubClaim_ClaimantEntityId.sql
-- Purpose: Add ClaimantEntityId to SubClaims for direct entity linking
-- Date: January 2025
-- =============================================

USE ClaimsPortal;
GO

PRINT '=== Adding ClaimantEntityId and Status Fields to SubClaims ===';
PRINT '';

-- =============================================
-- STEP 1: Add ClaimantEntityId to SubClaims
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ClaimantEntityId')
BEGIN
    ALTER TABLE SubClaims
    ADD ClaimantEntityId BIGINT NULL;
    
    PRINT 'Added ClaimantEntityId column to SubClaims table';
END
GO

-- =============================================
-- STEP 1b: Add AssignedAdjusterName to SubClaims (denormalized for display)
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'AssignedAdjusterName')
BEGIN
    ALTER TABLE SubClaims
    ADD AssignedAdjusterName NVARCHAR(200) NULL;
    
    PRINT 'Added AssignedAdjusterName column to SubClaims table';
END
GO

-- =============================================
-- STEP 1c: Add ClosedBy to SubClaims
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ClosedBy')
BEGIN
    ALTER TABLE SubClaims
    ADD ClosedBy NVARCHAR(100) NULL;
    
    PRINT 'Added ClosedBy column to SubClaims table';
END
GO

-- =============================================
-- STEP 1d: Add ReopenedDate to SubClaims
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ReopenedDate')
BEGIN
    ALTER TABLE SubClaims
    ADD ReopenedDate DATETIME NULL;
    
    PRINT 'Added ReopenedDate column to SubClaims table';
END
GO

-- =============================================
-- STEP 1e: Add ReopenedBy to SubClaims
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ReopenedBy')
BEGIN
    ALTER TABLE SubClaims
    ADD ReopenedBy NVARCHAR(100) NULL;
    
    PRINT 'Added ReopenedBy column to SubClaims table';
END
GO

-- =============================================
-- STEP 2: Add PartyType to EntityMaster (for better classification)
-- =============================================

-- PartyType already exists in your schema, but ensure it supports claim parties
-- Values: Vendor, Driver, Passenger, Pedestrian, Bicyclist, PropertyOwner, Attorney, Witness, ReportedBy

PRINT '';
PRINT 'PartyType values for claim parties:';
PRINT '  - Driver          (Insured driver)';
PRINT '  - Passenger       (Insured vehicle passenger)';
PRINT '  - ThirdPartyOwner (Third party vehicle owner)';
PRINT '  - ThirdPartyDriver(Third party driver)';
PRINT '  - Pedestrian      (Pedestrian involved)';
PRINT '  - Bicyclist       (Bicyclist involved)';
PRINT '  - PropertyOwner   (Property damage owner)';
PRINT '  - Attorney        (Legal representative)';
PRINT '  - Witness         (Witness to accident)';
PRINT '  - ReportedBy      (Person who reported claim)';
PRINT '';
GO

-- =============================================
-- STEP 3: Create View for Claimants with Entity and Address Details
-- =============================================

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_ClaimantFullDetails')
    DROP VIEW vw_ClaimantFullDetails;
GO

CREATE VIEW vw_ClaimantFullDetails
AS
SELECT 
    -- Claimant Info
    c.ClaimantId,
    c.FnolId,
    c.ClaimNumber,
    c.ClaimantName,
    c.ClaimantType,
    c.FeatureNumber,
    c.Coverage,
    c.CoverageDescription,
    c.CoverageLimits,
    c.Deductible,
    c.HasInjury,
    c.InjuryType,
    c.InjurySeverity,
    c.InjuryDescription,
    c.IsFatality,
    c.IsHospitalized,
    c.IsAttorneyRepresented,
    
    -- Entity Info (from EntityMaster)
    e.EntityId,
    e.EntityType,
    e.PartyType,
    e.EntityName,
    e.DBA,
    e.HomeBusinessPhone,
    e.MobilePhone,
    e.Email AS EntityEmail,
    e.FEINorSS,
    e.LicenseNumber,
    e.LicenseState,
    e.DateOfBirth,
    
    -- Primary Address (from AddressMaster)
    a.AddressId,
    a.StreetAddress,
    a.Apt AS AddressLine2,
    a.City,
    a.State,
    a.ZipCode,
    a.Country,
    
    -- Attorney Info (if represented)
    att.EntityId AS AttorneyEntityId,
    att.EntityName AS AttorneyName,
    att.DBA AS AttorneyFirmName,
    att.HomeBusinessPhone AS AttorneyPhone,
    att.Email AS AttorneyEmail,
    
    -- Metadata
    c.CreatedDate,
    c.CreatedBy
    
FROM Claimants c
LEFT JOIN EntityMaster e ON c.ClaimantEntityId = e.EntityId
LEFT JOIN AddressMaster a ON e.EntityId = a.EntityId AND a.AddressType = 'M' AND a.AddressStatus = 'Y'
LEFT JOIN EntityMaster att ON c.AttorneyEntityId = att.EntityId;
GO

PRINT 'Created vw_ClaimantFullDetails view';
GO

-- =============================================
-- STEP 4: Create View for SubClaims with Claimant Entity Details
-- =============================================

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_SubClaimWithEntity')
    DROP VIEW vw_SubClaimWithEntity;
GO

CREATE VIEW vw_SubClaimWithEntity
AS
SELECT 
    -- SubClaim Info
    sc.SubClaimId,
    sc.FnolId,
    sc.ClaimNumber,
    sc.SubClaimNumber,
    sc.FeatureNumber,
    sc.ClaimantName,
    sc.ClaimantType,
    sc.Coverage,
    sc.CoverageLimits,
    sc.SubClaimStatus,
    sc.OpenedDate,
    sc.ClosedDate,
    sc.ClosedBy,
    sc.ReopenedDate,
    sc.ReopenedBy,
    sc.IndemnityReserve,
    sc.IndemnityPaid,
    sc.ExpenseReserve,
    sc.ExpensePaid,
    sc.AssignedAdjusterId,
    sc.AssignedAdjusterName,
    
    -- Entity Info (direct link from SubClaim)
    e.EntityId AS ClaimantEntityId,
    e.EntityType,
    e.PartyType,
    e.EntityName,
    e.HomeBusinessPhone AS ClaimantPhone,
    e.Email AS ClaimantEmail,
    e.FEINorSS,
    
    -- Primary Address
    a.StreetAddress,
    a.Apt AS AddressLine2,
    a.City,
    a.State,
    a.ZipCode,
    
    -- Metadata
    sc.CreatedDate,
    sc.CreatedBy
    
FROM SubClaims sc
LEFT JOIN EntityMaster e ON sc.ClaimantEntityId = e.EntityId
LEFT JOIN AddressMaster a ON e.EntityId = a.EntityId AND a.AddressType = 'M' AND a.AddressStatus = 'Y';
GO

PRINT 'Created vw_SubClaimWithEntity view';
GO

-- =============================================
-- STEP 5: Create Stored Procedure to Create Claimant with Entity
-- =============================================

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_CreateClaimantWithEntity')
    DROP PROCEDURE sp_CreateClaimantWithEntity;
GO

CREATE PROCEDURE sp_CreateClaimantWithEntity
    -- FNOL/Claim Info
    @FnolId BIGINT,
    @ClaimNumber NVARCHAR(50),
    @ClaimantType NVARCHAR(50),
    
    -- Person Info
    @Name NVARCHAR(200),
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @FeinSsNumber NVARCHAR(50) = NULL,
    @LicenseNumber NVARCHAR(50) = NULL,
    @LicenseState NVARCHAR(10) = NULL,
    @DateOfBirth DATE = NULL,
    
    -- Address Info
    @StreetAddress NVARCHAR(200) = NULL,
    @AddressLine2 NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(10) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    
    -- Injury Info
    @HasInjury BIT = 0,
    @InjuryType NVARCHAR(100) = NULL,
    @InjurySeverity NVARCHAR(20) = NULL,
    @InjuryDescription NVARCHAR(MAX) = NULL,
    @IsFatality BIT = 0,
    
    -- Attorney Info
    @IsAttorneyRepresented BIT = 0,
    @AttorneyEntityId BIGINT = NULL,
    
    @CreatedBy NVARCHAR(100) = NULL,
    
    -- Outputs
    @EntityId BIGINT OUTPUT,
    @ClaimantId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if entity already exists (by name + FEIN or name + license)
        SELECT @EntityId = EntityId 
        FROM EntityMaster 
        WHERE EntityName = @Name 
          AND ((@FeinSsNumber IS NOT NULL AND FEINorSS = @FeinSsNumber)
               OR (@LicenseNumber IS NOT NULL AND LicenseNumber = @LicenseNumber AND LicenseState = @LicenseState));
        
        -- Create new entity if not found
        IF @EntityId IS NULL
        BEGIN
            INSERT INTO EntityMaster (
                EntityType, PartyType, EntityGroupCode, EntityName,
                HomeBusinessPhone, MobilePhone, Email, FEINorSS,
                LicenseNumber, LicenseState, DateOfBirth,
                EntityStatus, CreatedBy, CreatedDate
            )
            VALUES (
                'I', -- Individual
                @ClaimantType,
                'Claimant',
                @Name,
                @Phone,
                @Phone,
                @Email,
                @FeinSsNumber,
                @LicenseNumber,
                @LicenseState,
                @DateOfBirth,
                'Y',
                @CreatedBy,
                GETDATE()
            );
            
            SET @EntityId = SCOPE_IDENTITY();
            
            -- Create address if provided
            IF @StreetAddress IS NOT NULL OR @City IS NOT NULL OR @State IS NOT NULL
            BEGIN
                INSERT INTO AddressMaster (
                    EntityId, AddressType, StreetAddress, Apt, City, State, ZipCode,
                    HomeBusinessPhone, Email, AddressStatus, CreatedBy, CreatedDate
                )
                VALUES (
                    @EntityId, 'M', @StreetAddress, @AddressLine2, @City, @State, @ZipCode,
                    @Phone, @Email, 'Y', @CreatedBy, GETDATE()
                );
            END
        END
        
        -- Create Claimant record
        INSERT INTO Claimants (
            FnolId, ClaimNumber, ClaimantEntityId, ClaimantName, ClaimantType,
            HasInjury, InjuryType, InjurySeverity, InjuryDescription, IsFatality,
            IsAttorneyRepresented, AttorneyEntityId, CreatedBy, CreatedDate
        )
        VALUES (
            @FnolId, @ClaimNumber, @EntityId, @Name, @ClaimantType,
            @HasInjury, @InjuryType, @InjurySeverity, @InjuryDescription, @IsFatality,
            @IsAttorneyRepresented, @AttorneyEntityId, @CreatedBy, GETDATE()
        );
        
        SET @ClaimantId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Created sp_CreateClaimantWithEntity stored procedure';
GO

-- =============================================
-- STEP 6: Create Index on ClaimantEntityId
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SubClaims_ClaimantEntityId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubClaims_ClaimantEntityId 
    ON SubClaims(ClaimantEntityId) 
    WHERE ClaimantEntityId IS NOT NULL;
    
    PRINT 'Created index IX_SubClaims_ClaimantEntityId';
END
GO

-- =============================================
-- Summary
-- =============================================

PRINT '';
PRINT '=== Migration Complete ===';
PRINT '';
PRINT 'Changes:';
PRINT '  1. Added SubClaims.ClaimantEntityId (FK to EntityMaster)';
PRINT '  2. Added SubClaims.AssignedAdjusterName (denormalized)';
PRINT '  3. Added SubClaims.ClosedBy';
PRINT '  4. Added SubClaims.ReopenedDate';
PRINT '  5. Added SubClaims.ReopenedBy';
PRINT '  6. Created vw_ClaimantFullDetails view';
PRINT '  7. Created vw_SubClaimWithEntity view';
PRINT '  8. Created sp_CreateClaimantWithEntity stored procedure';
PRINT '  9. Created index IX_SubClaims_ClaimantEntityId';
PRINT '';
PRINT 'Data Flow:';
PRINT '  Person -> EntityMaster (get EntityId)';
PRINT '  Address -> AddressMaster (link via EntityId)';
PRINT '  Claimant -> Claimants (link ClaimantEntityId to EntityId)';
PRINT '  SubClaim -> SubClaims (link ClaimantEntityId to EntityId)';
PRINT '';
GO
