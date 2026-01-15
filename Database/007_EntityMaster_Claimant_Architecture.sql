-- =============================================
-- Migration Script: 007_EntityMaster_Claimant_Architecture.sql
-- Purpose: Implement EntityMaster pattern for centralized entity/address management
-- Date: January 2025
-- =============================================

USE ClaimsPortal;
GO

PRINT '=== Starting EntityMaster Architecture Migration ===';
PRINT '';

-- =============================================
-- STEP 1: Create EntityMaster Table
-- Central registry for all persons/entities
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityMaster')
BEGIN
    CREATE TABLE EntityMaster (
        EntityId INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Entity Classification
        EntityType NVARCHAR(50) NOT NULL,  -- Driver, Passenger, ThirdPartyDriver, Attorney, Witness, etc.
        EntitySubType NVARCHAR(50) NULL,   -- Additional classification
        EntityCategory NVARCHAR(20) NOT NULL DEFAULT 'Individual', -- Individual or Business
        
        -- Name Fields
        Name NVARCHAR(200) NOT NULL,
        FirstName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NULL,
        MiddleName NVARCHAR(50) NULL,
        Suffix NVARCHAR(10) NULL,
        
        -- Business Fields
        BusinessName NVARCHAR(200) NULL,
        DoingBusinessAs NVARCHAR(200) NULL,
        
        -- Contact Information
        PhoneNumber NVARCHAR(20) NULL,
        Phone2 NVARCHAR(20) NULL,
        Email NVARCHAR(200) NULL,
        
        -- Identification
        FeinSsNumber NVARCHAR(50) NULL,  -- Encrypted/masked
        LicenseNumber NVARCHAR(50) NULL,
        LicenseState NVARCHAR(10) NULL,
        
        -- Personal Information
        DateOfBirth DATE NULL,
        Gender NVARCHAR(10) NULL,
        
        -- Attorney-Specific
        FirmName NVARCHAR(200) NULL,
        BarNumber NVARCHAR(50) NULL,
        BarState NVARCHAR(10) NULL,
        
        -- Metadata
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME NULL,
        ModifiedBy NVARCHAR(100) NULL
    );
    
    -- Create indexes for common queries
    CREATE NONCLUSTERED INDEX IX_EntityMaster_EntityType ON EntityMaster(EntityType);
    CREATE NONCLUSTERED INDEX IX_EntityMaster_Name ON EntityMaster(Name);
    CREATE NONCLUSTERED INDEX IX_EntityMaster_FeinSsNumber ON EntityMaster(FeinSsNumber) WHERE FeinSsNumber IS NOT NULL;
    CREATE NONCLUSTERED INDEX IX_EntityMaster_LicenseNumber ON EntityMaster(LicenseNumber) WHERE LicenseNumber IS NOT NULL;
    
    PRINT 'Created EntityMaster table';
END
ELSE
BEGIN
    PRINT 'EntityMaster table already exists';
END
GO

-- =============================================
-- STEP 2: Create AddressMaster Table
-- Centralized address storage linked to EntityMaster
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AddressMaster')
BEGIN
    CREATE TABLE AddressMaster (
        AddressId INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Link to Entity
        EntityId INT NOT NULL,
        
        -- Address Classification
        AddressType NVARCHAR(20) NOT NULL DEFAULT 'Home', -- Home, Business, Mailing, Property, etc.
        IsPrimary BIT NOT NULL DEFAULT 1,
        
        -- Address Fields
        StreetAddress NVARCHAR(200) NULL,
        AddressLine2 NVARCHAR(100) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(10) NULL,
        ZipCode NVARCHAR(20) NULL,
        County NVARCHAR(100) NULL,
        Country NVARCHAR(50) NOT NULL DEFAULT 'USA',
        
        -- Geocoding
        Latitude DECIMAL(10,7) NULL,
        Longitude DECIMAL(10,7) NULL,
        GeoAccuracy NVARCHAR(50) NULL,
        IsVerified BIT NOT NULL DEFAULT 0,
        VerifiedDate DATETIME NULL,
        
        -- Validity Period
        EffectiveDate DATE NULL,
        EndDate DATE NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        
        -- Metadata
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME NULL,
        ModifiedBy NVARCHAR(100) NULL,
        
        -- Foreign Key
        CONSTRAINT FK_AddressMaster_EntityMaster FOREIGN KEY (EntityId) 
            REFERENCES EntityMaster(EntityId) ON DELETE CASCADE
    );
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX IX_AddressMaster_EntityId ON AddressMaster(EntityId);
    CREATE NONCLUSTERED INDEX IX_AddressMaster_AddressType ON AddressMaster(EntityId, AddressType);
    CREATE NONCLUSTERED INDEX IX_AddressMaster_City ON AddressMaster(City, State);
    CREATE NONCLUSTERED INDEX IX_AddressMaster_ZipCode ON AddressMaster(ZipCode);
    
    PRINT 'Created AddressMaster table';
END
ELSE
BEGIN
    PRINT 'AddressMaster table already exists';
END
GO

-- =============================================
-- STEP 3: Create Claimants Table
-- Links entities to claims as claimants
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Claimants')
BEGIN
    CREATE TABLE Claimants (
        ClaimantId INT IDENTITY(1,1) PRIMARY KEY,
        
        -- Claim Reference
        ClaimId INT NOT NULL,
        ClaimNumber NVARCHAR(50) NULL,
        
        -- Entity Reference
        EntityId INT NOT NULL,
        ClaimantName NVARCHAR(200) NOT NULL, -- Denormalized for performance
        
        -- Claimant Classification
        ClaimantType NVARCHAR(50) NOT NULL, -- Driver, Passenger, Pedestrian, etc.
        ClaimantRole NVARCHAR(100) NULL,    -- Descriptive role
        
        -- Status
        Status NVARCHAR(20) NOT NULL DEFAULT 'Open',
        
        -- Injury Information
        WasInjured BIT NOT NULL DEFAULT 0,
        InjuryId INT NULL,
        InjuryType NVARCHAR(100) NULL,
        InjurySeverity INT NULL,
        IsFatality BIT NOT NULL DEFAULT 0,
        
        -- Attorney Information
        HasAttorney BIT NOT NULL DEFAULT 0,
        AttorneyEntityId INT NULL,
        
        -- Claim-Specific Contact Override
        ClaimPhoneNumber NVARCHAR(20) NULL,
        ClaimEmail NVARCHAR(200) NULL,
        ClaimAddressId INT NULL,
        
        -- Metadata
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME NULL,
        ModifiedBy NVARCHAR(100) NULL,
        
        -- Foreign Keys
        CONSTRAINT FK_Claimants_FNOL FOREIGN KEY (ClaimId) 
            REFERENCES FNOL(FnolId) ON DELETE CASCADE,
        CONSTRAINT FK_Claimants_EntityMaster FOREIGN KEY (EntityId) 
            REFERENCES EntityMaster(EntityId),
        CONSTRAINT FK_Claimants_AttorneyEntity FOREIGN KEY (AttorneyEntityId) 
            REFERENCES EntityMaster(EntityId),
        CONSTRAINT FK_Claimants_ClaimAddress FOREIGN KEY (ClaimAddressId) 
            REFERENCES AddressMaster(AddressId)
    );
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX IX_Claimants_ClaimId ON Claimants(ClaimId);
    CREATE NONCLUSTERED INDEX IX_Claimants_EntityId ON Claimants(EntityId);
    CREATE NONCLUSTERED INDEX IX_Claimants_ClaimNumber ON Claimants(ClaimNumber);
    CREATE NONCLUSTERED INDEX IX_Claimants_ClaimantType ON Claimants(ClaimId, ClaimantType);
    
    PRINT 'Created Claimants table';
END
ELSE
BEGIN
    PRINT 'Claimants table already exists';
END
GO

-- =============================================
-- STEP 4: Add ClaimantId to SubClaims Table
-- Link SubClaims to Claimants instead of just name
-- =============================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubClaims' AND COLUMN_NAME = 'ClaimantId')
BEGIN
    ALTER TABLE SubClaims
    ADD ClaimantId INT NULL;
    
    PRINT 'Added ClaimantId column to SubClaims table';
END
GO

-- Add foreign key after data migration
-- CONSTRAINT FK_SubClaims_Claimants FOREIGN KEY (ClaimantId) REFERENCES Claimants(ClaimantId)

-- =============================================
-- STEP 5: Create View for Claimant Details
-- Easy access to claimant info with address
-- =============================================

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_ClaimantDetails')
    DROP VIEW vw_ClaimantDetails;
GO

CREATE VIEW vw_ClaimantDetails
AS
SELECT 
    c.ClaimantId,
    c.ClaimId,
    c.ClaimNumber,
    c.ClaimantName,
    c.ClaimantType,
    c.ClaimantRole,
    c.Status AS ClaimantStatus,
    c.WasInjured,
    c.InjuryType,
    c.InjurySeverity,
    c.IsFatality,
    c.HasAttorney,
    
    -- Entity Details
    e.EntityId,
    e.EntityType,
    e.EntityCategory,
    e.FirstName,
    e.LastName,
    e.PhoneNumber,
    e.Email,
    e.DateOfBirth,
    e.FeinSsNumber,
    e.LicenseNumber,
    e.LicenseState,
    
    -- Primary Address
    a.AddressId,
    a.StreetAddress,
    a.AddressLine2,
    a.City,
    a.State,
    a.ZipCode,
    a.County,
    
    -- Attorney Details (if has attorney)
    att.Name AS AttorneyName,
    att.FirmName AS AttorneyFirmName,
    att.PhoneNumber AS AttorneyPhone,
    att.Email AS AttorneyEmail,
    
    c.CreatedDate,
    c.CreatedBy
    
FROM Claimants c
INNER JOIN EntityMaster e ON c.EntityId = e.EntityId
LEFT JOIN AddressMaster a ON e.EntityId = a.EntityId AND a.IsPrimary = 1 AND a.IsActive = 1
LEFT JOIN EntityMaster att ON c.AttorneyEntityId = att.EntityId;
GO

PRINT 'Created vw_ClaimantDetails view';
GO

-- =============================================
-- STEP 6: Create View for SubClaim with Claimant
-- =============================================

IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_SubClaimWithClaimant')
    DROP VIEW vw_SubClaimWithClaimant;
GO

CREATE VIEW vw_SubClaimWithClaimant
AS
SELECT 
    sc.SubClaimId,
    sc.FnolId AS ClaimId,
    f.ClaimNumber,
    sc.FeatureNumber,
    sc.Coverage,
    sc.CoverageLimits,
    sc.ExpenseReserve,
    sc.IndemnityReserve,
    sc.ExpensePaid,
    sc.IndemnityPaid,
    sc.Deductible,
    sc.SubClaimStatus,
    sc.AssignedAdjusterId,
    sc.AssignedAdjusterName,
    sc.OpenedDate,
    sc.ClosedDate,
    sc.ClosedBy,
    sc.ReopenedDate,
    sc.ReopenedBy,
    
    -- Claimant Info (from ClaimantId if available, otherwise from ClaimantName)
    c.ClaimantId,
    COALESCE(c.ClaimantName, sc.ClaimantName) AS ClaimantName,
    c.ClaimantType,
    c.WasInjured,
    c.HasAttorney,
    
    -- Entity Details (if linked)
    e.EntityId,
    e.PhoneNumber AS ClaimantPhone,
    e.Email AS ClaimantEmail,
    
    -- Primary Address (if linked)
    a.StreetAddress,
    a.City,
    a.State,
    a.ZipCode
    
FROM SubClaims sc
INNER JOIN FNOL f ON sc.FnolId = f.FnolId
LEFT JOIN Claimants c ON sc.ClaimantId = c.ClaimantId
LEFT JOIN EntityMaster e ON c.EntityId = e.EntityId
LEFT JOIN AddressMaster a ON e.EntityId = a.EntityId AND a.IsPrimary = 1 AND a.IsActive = 1;
GO

PRINT 'Created vw_SubClaimWithClaimant view';
GO

-- =============================================
-- STEP 7: Create Stored Procedure to Add Entity
-- =============================================

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_CreateEntity')
    DROP PROCEDURE sp_CreateEntity;
GO

CREATE PROCEDURE sp_CreateEntity
    @EntityType NVARCHAR(50),
    @Name NVARCHAR(200),
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @EntityCategory NVARCHAR(20) = 'Individual',
    @PhoneNumber NVARCHAR(20) = NULL,
    @Email NVARCHAR(200) = NULL,
    @FeinSsNumber NVARCHAR(50) = NULL,
    @LicenseNumber NVARCHAR(50) = NULL,
    @LicenseState NVARCHAR(10) = NULL,
    @DateOfBirth DATE = NULL,
    @FirmName NVARCHAR(200) = NULL,
    @CreatedBy NVARCHAR(100) = NULL,
    -- Address fields
    @StreetAddress NVARCHAR(200) = NULL,
    @AddressLine2 NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(10) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    @County NVARCHAR(100) = NULL,
    -- Output
    @EntityId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insert Entity
        INSERT INTO EntityMaster (
            EntityType, Name, FirstName, LastName, EntityCategory,
            PhoneNumber, Email, FeinSsNumber, LicenseNumber, LicenseState,
            DateOfBirth, FirmName, CreatedBy, CreatedDate
        )
        VALUES (
            @EntityType, @Name, @FirstName, @LastName, @EntityCategory,
            @PhoneNumber, @Email, @FeinSsNumber, @LicenseNumber, @LicenseState,
            @DateOfBirth, @FirmName, @CreatedBy, GETDATE()
        );
        
        SET @EntityId = SCOPE_IDENTITY();
        
        -- Insert Address if provided
        IF @StreetAddress IS NOT NULL OR @City IS NOT NULL OR @State IS NOT NULL OR @ZipCode IS NOT NULL
        BEGIN
            INSERT INTO AddressMaster (
                EntityId, AddressType, IsPrimary,
                StreetAddress, AddressLine2, City, State, ZipCode, County,
                CreatedBy, CreatedDate
            )
            VALUES (
                @EntityId, 'Home', 1,
                @StreetAddress, @AddressLine2, @City, @State, @ZipCode, @County,
                @CreatedBy, GETDATE()
            );
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Created sp_CreateEntity stored procedure';
GO

-- =============================================
-- STEP 8: Create Stored Procedure to Add Claimant
-- =============================================

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_CreateClaimant')
    DROP PROCEDURE sp_CreateClaimant;
GO

CREATE PROCEDURE sp_CreateClaimant
    @ClaimId INT,
    @ClaimNumber NVARCHAR(50),
    @EntityId INT,
    @ClaimantType NVARCHAR(50),
    @ClaimantRole NVARCHAR(100) = NULL,
    @WasInjured BIT = 0,
    @InjuryType NVARCHAR(100) = NULL,
    @InjurySeverity INT = NULL,
    @IsFatality BIT = 0,
    @HasAttorney BIT = 0,
    @AttorneyEntityId INT = NULL,
    @CreatedBy NVARCHAR(100) = NULL,
    -- Output
    @ClaimantId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ClaimantName NVARCHAR(200);
    
    -- Get name from EntityMaster
    SELECT @ClaimantName = Name FROM EntityMaster WHERE EntityId = @EntityId;
    
    -- Insert Claimant
    INSERT INTO Claimants (
        ClaimId, ClaimNumber, EntityId, ClaimantName, ClaimantType, ClaimantRole,
        WasInjured, InjuryType, InjurySeverity, IsFatality,
        HasAttorney, AttorneyEntityId, CreatedBy, CreatedDate
    )
    VALUES (
        @ClaimId, @ClaimNumber, @EntityId, @ClaimantName, @ClaimantType, @ClaimantRole,
        @WasInjured, @InjuryType, @InjurySeverity, @IsFatality,
        @HasAttorney, @AttorneyEntityId, @CreatedBy, GETDATE()
    );
    
    SET @ClaimantId = SCOPE_IDENTITY();
END
GO

PRINT 'Created sp_CreateClaimant stored procedure';
GO

-- =============================================
-- Verify the changes
-- =============================================

PRINT '';
PRINT '=== Migration Complete ===';
PRINT '';
PRINT 'New Tables Created:';
PRINT '  - EntityMaster (central entity registry)';
PRINT '  - AddressMaster (centralized addresses)';
PRINT '  - Claimants (links entities to claims)';
PRINT '';
PRINT 'New Views Created:';
PRINT '  - vw_ClaimantDetails (claimant with entity and address info)';
PRINT '  - vw_SubClaimWithClaimant (subclaim with claimant details)';
PRINT '';
PRINT 'New Stored Procedures:';
PRINT '  - sp_CreateEntity (creates entity with optional address)';
PRINT '  - sp_CreateClaimant (creates claimant linked to entity)';
PRINT '';
PRINT 'Schema Updated:';
PRINT '  - SubClaims.ClaimantId added (FK to Claimants)';
GO
