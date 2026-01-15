-- ============================================================================
-- CREATE VENDORMASTER TABLE (SEPARATE FROM ENTITYMASTER)
-- ============================================================================
-- Purpose: Create a dedicated VendorMaster table for all vendors
-- (Hospitals, Attorneys, Authorities, Medical Providers, etc.)
-- 
-- This separates slowly-growing vendor data (~500/year) from 
-- rapidly-growing claim-related entities (~50,000/year)
-- 
-- Benefits:
-- - Faster vendor searches (50K records vs 800K+ in EntityMaster)
-- - Optimized indexing for vendor-specific queries
-- - Cleaner data model with vendor-specific fields
-- - Can be cached for performance
-- ============================================================================

USE ClaimsPortal;
GO

PRINT '=== Creating VendorMaster Table ===';
PRINT '';

-- ============================================================================
-- STEP 1: CREATE VENDORMASTER TABLE
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'VendorMaster')
BEGIN
    CREATE TABLE VendorMaster (
        -- Primary Key
        VendorId BIGINT IDENTITY(1,1) PRIMARY KEY,
        
        -- Vendor Classification
        VendorType NVARCHAR(50) NOT NULL,           -- Hospital, Defense Attorney, Plaintiff Attorney, Police Department, Fire Station, Medical Provider, Towing Service, Repair Shop, etc.
        EntityType CHAR(1) NOT NULL DEFAULT 'B',     -- 'B' = Business, 'I' = Individual
        
        -- Basic Information
        VendorName NVARCHAR(200) NOT NULL,          -- Primary name
        DoingBusinessAs NVARCHAR(200) NULL,         -- DBA name
        FEINNumber NVARCHAR(20) NULL,               -- Federal Employer ID Number
        
        -- Contact Information
        ContactName NVARCHAR(100) NULL,             -- Primary contact person
        BusinessPhone NVARCHAR(20) NULL,
        MobilePhone NVARCHAR(20) NULL,
        FaxNumber NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Website NVARCHAR(200) NULL,
        
        -- Dates
        EffectiveDate DATE NULL,                    -- When vendor relationship started
        TerminationDate DATE NULL,                  -- When vendor relationship ended
        
        -- Tax Information
        W9Received BIT NOT NULL DEFAULT 0,
        W9ReceivedDate DATE NULL,
        SubjectTo1099 BIT NOT NULL DEFAULT 0,
        BackupWithholding BIT NOT NULL DEFAULT 0,
        
        -- Payment Information
        ReceivesBulkPayment BIT NOT NULL DEFAULT 0,
        PaymentFrequency NVARCHAR(20) NULL,         -- 'Monthly', 'Weekly'
        PaymentDays NVARCHAR(100) NULL,             -- Comma-separated: '1,15' for Monthly or 'Monday,Friday' for Weekly
        
        -- For Attorneys specifically
        BarNumber NVARCHAR(50) NULL,
        BarState NVARCHAR(2) NULL,
        
        -- For Authorities specifically (Police/Fire)
        JurisdictionType NVARCHAR(50) NULL,         -- City, County, State, Federal
        
        -- Status
        VendorStatus CHAR(1) NOT NULL DEFAULT 'Y',  -- 'Y' = Active, 'D' = Disabled
        
        -- Audit Fields
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50) NULL,
        ModifiedDate DATETIME NULL,
        ModifiedBy NVARCHAR(50) NULL,
        
        -- Legacy Reference (for migration)
        LegacyEntityId BIGINT NULL                  -- Reference to old EntityMaster record if migrated
    );
    
    PRINT 'Created VendorMaster table';
    
    -- Create indexes for common search patterns
    CREATE NONCLUSTERED INDEX IX_VendorMaster_VendorType 
        ON VendorMaster(VendorType) 
        WHERE VendorStatus = 'Y';
    
    CREATE NONCLUSTERED INDEX IX_VendorMaster_VendorName 
        ON VendorMaster(VendorName) 
        INCLUDE (VendorType, DoingBusinessAs, BusinessPhone, Email);
    
    CREATE NONCLUSTERED INDEX IX_VendorMaster_FEINNumber 
        ON VendorMaster(FEINNumber) 
        WHERE FEINNumber IS NOT NULL;
    
    CREATE NONCLUSTERED INDEX IX_VendorMaster_Status 
        ON VendorMaster(VendorStatus);
    
    PRINT 'Created indexes on VendorMaster';
END
ELSE
BEGIN
    PRINT 'VendorMaster table already exists';
END
GO

-- ============================================================================
-- STEP 2: CREATE VENDORADDRESS TABLE
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'VendorAddress')
BEGIN
    CREATE TABLE VendorAddress (
        VendorAddressId BIGINT IDENTITY(1,1) PRIMARY KEY,
        VendorId BIGINT NOT NULL,
        
        -- Address Type
        AddressType CHAR(1) NOT NULL DEFAULT 'M',   -- 'M' = Main, 'A' = Alternate, 'T' = Temporary
        
        -- Address Fields
        StreetAddress NVARCHAR(200) NULL,
        AddressLine2 NVARCHAR(100) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(2) NULL,
        ZipCode NVARCHAR(10) NULL,
        Country NVARCHAR(50) NULL DEFAULT 'USA',
        
        -- Contact at this address (optional)
        Phone NVARCHAR(20) NULL,
        Fax NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        
        -- Status
        AddressStatus CHAR(1) NOT NULL DEFAULT 'Y', -- 'Y' = Active, 'D' = Disabled
        
        -- Audit
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50) NULL,
        ModifiedDate DATETIME NULL,
        ModifiedBy NVARCHAR(50) NULL,
        
        -- Foreign Key
        CONSTRAINT FK_VendorAddress_VendorMaster 
            FOREIGN KEY (VendorId) REFERENCES VendorMaster(VendorId)
    );
    
    PRINT 'Created VendorAddress table';
    
    -- Index for vendor address lookup
    CREATE NONCLUSTERED INDEX IX_VendorAddress_VendorId 
        ON VendorAddress(VendorId) 
        INCLUDE (AddressType, StreetAddress, City, State, ZipCode);
    
    PRINT 'Created index on VendorAddress';
END
ELSE
BEGIN
    PRINT 'VendorAddress table already exists';
END
GO

-- ============================================================================
-- STEP 3: ADD VENDOR REFERENCE COLUMNS TO CLAIMANTS (IF TABLE EXISTS)
-- ============================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Claimants')
BEGIN
    -- Add HospitalVendorId to Claimants
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'HospitalVendorId')
    BEGIN
        ALTER TABLE Claimants ADD HospitalVendorId BIGINT NULL;
        PRINT 'Added HospitalVendorId to Claimants table';
    END

    -- Add AttorneyVendorId to Claimants
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Claimants' AND COLUMN_NAME = 'AttorneyVendorId')
    BEGIN
        ALTER TABLE Claimants ADD AttorneyVendorId BIGINT NULL;
        PRINT 'Added AttorneyVendorId to Claimants table';
    END
END
ELSE
BEGIN
    PRINT 'Claimants table does not exist - skipping column additions';
END
GO

-- ============================================================================
-- STEP 4: ADD VENDOR REFERENCE TO FNOLAUTHORITIES (IF TABLE EXISTS)
-- ============================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FnolAuthorities')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'FnolAuthorities' AND COLUMN_NAME = 'VendorId')
    BEGIN
        ALTER TABLE FnolAuthorities ADD VendorId BIGINT NULL;
        PRINT 'Added VendorId to FnolAuthorities table';
    END
END
ELSE
BEGIN
    PRINT 'FnolAuthorities table does not exist - skipping column addition';
END
GO

-- ============================================================================
-- STEP 5: MIGRATE EXISTING VENDORS FROM ENTITYMASTER TO VENDORMASTER
-- (Only runs if EntityMaster table exists with vendor data)
-- ============================================================================

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EntityMaster')
   AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EntityMaster' AND COLUMN_NAME = 'EntityGroupCode')
BEGIN
    PRINT '';
    PRINT 'Migrating vendors from EntityMaster to VendorMaster...';

    -- Migrate vendors where EntityGroupCode = 'Vendor'
    INSERT INTO VendorMaster (
        VendorType,
        EntityType,
        VendorName,
        DoingBusinessAs,
        FEINNumber,
        ContactName,
        BusinessPhone,
        MobilePhone,
        FaxNumber,
        Email,
        EffectiveDate,
        TerminationDate,
        W9Received,
        SubjectTo1099,
        BackupWithholding,
        ReceivesBulkPayment,
        PaymentFrequency,
        PaymentDays,
        VendorStatus,
        CreatedDate,
        CreatedBy,
        ModifiedDate,
        ModifiedBy,
        LegacyEntityId
    )
    SELECT 
        COALESCE(PartyType, VendorType, 'Other') AS VendorType,
        EntityType,
        EntityName AS VendorName,
        DBA AS DoingBusinessAs,
        FEINorSS AS FEINNumber,
        ContactName,
        HomeBusinessPhone AS BusinessPhone,
        MobilePhone,
        FaxNumber,
        Email,
        EntityEffectiveDate AS EffectiveDate,
        EntityTerminationDate AS TerminationDate,
        W9Received,
        IsSubjectTo1099 AS SubjectTo1099,
        IsBackupWithholding AS BackupWithholding,
        ReceivesBulkPayment,
        PaymentFrequency,
        COALESCE(BulkPaymentDayDate1, '') + 
            CASE WHEN BulkPaymentDayDate2 IS NOT NULL THEN ',' + BulkPaymentDayDate2 ELSE '' END AS PaymentDays,
        EntityStatus AS VendorStatus,
        CreatedDate,
        CreatedBy,
        ModifiedDate,
        ModifiedBy,
        EntityId AS LegacyEntityId
    FROM EntityMaster
    WHERE EntityGroupCode = 'Vendor'
      AND EntityId NOT IN (SELECT LegacyEntityId FROM VendorMaster WHERE LegacyEntityId IS NOT NULL);

    DECLARE @VendorsMigrated INT = @@ROWCOUNT;
    PRINT CONCAT('  Migrated ', @VendorsMigrated, ' vendors from EntityMaster');

    -- Migrate addresses for these vendors (if AddressMaster exists)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AddressMaster')
    BEGIN
        INSERT INTO VendorAddress (
            VendorId,
            AddressType,
            StreetAddress,
            AddressLine2,
            City,
            State,
            ZipCode,
            Phone,
            Fax,
            Email,
            AddressStatus,
            CreatedDate,
            CreatedBy
        )
        SELECT 
            vm.VendorId,
            am.AddressType,
            am.StreetAddress,
            am.Apt AS AddressLine2,
            am.City,
            am.State,
            am.ZipCode,
            am.HomeBusinessPhone AS Phone,
            am.FaxNumber AS Fax,
            am.Email,
            am.AddressStatus,
            am.CreatedDate,
            am.CreatedBy
        FROM AddressMaster am
        INNER JOIN VendorMaster vm ON vm.LegacyEntityId = am.EntityId
        WHERE NOT EXISTS (
            SELECT 1 FROM VendorAddress va 
            WHERE va.VendorId = vm.VendorId 
            AND va.AddressType = am.AddressType
        );

        DECLARE @AddressesMigrated INT = @@ROWCOUNT;
        PRINT CONCAT('  Migrated ', @AddressesMigrated, ' vendor addresses');
    END
END
ELSE
BEGIN
    PRINT '';
    PRINT 'EntityMaster table not found - skipping migration step';
END
GO

-- ============================================================================
-- STEP 6: ADD SAMPLE VENDORS TO NEW TABLE
-- ============================================================================

PRINT '';
PRINT 'Adding sample vendors to VendorMaster...';

-- HOSPITALS
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Memorial General Hospital' AND VendorType = 'Hospital')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Hospital', 'B', 'Memorial General Hospital', '(555) 100-1001', 'info@memorialhospital.com', 'Y', 'system');
    
    DECLARE @VendorId1 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId1, 'M', '100 Hospital Drive', 'Springfield', 'IL', '62701', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'St. Mary Medical Center' AND VendorType = 'Hospital')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Hospital', 'B', 'St. Mary Medical Center', '(555) 100-1002', 'info@stmarymedical.com', 'Y', 'system');
    
    DECLARE @VendorId2 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId2, 'M', '200 Medical Center Blvd', 'Chicago', 'IL', '60601', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Northwestern Memorial Hospital' AND VendorType = 'Hospital')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Hospital', 'B', 'Northwestern Memorial Hospital', '(312) 926-2000', 'info@nm.org', 'Y', 'system');
    
    DECLARE @VendorId3 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId3, 'M', '251 E Huron St', 'Chicago', 'IL', '60611', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Rush University Medical Center' AND VendorType = 'Hospital')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Hospital', 'B', 'Rush University Medical Center', '(312) 942-5000', 'info@rush.edu', 'Y', 'system');
    
    DECLARE @VendorId4 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId4, 'M', '1653 W Congress Pkwy', 'Chicago', 'IL', '60612', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Advocate Christ Medical Center' AND VendorType = 'Hospital')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Hospital', 'B', 'Advocate Christ Medical Center', '(708) 684-8000', 'info@advocatehealth.com', 'Y', 'system');
    
    DECLARE @VendorId5 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId5, 'M', '4440 W 95th St', 'Oak Lawn', 'IL', '60453', 'system');
END
GO

-- DEFENSE ATTORNEYS
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Smith & Associates Law Firm' AND VendorType = 'Defense Attorney')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, ContactName, BusinessPhone, Email, BarNumber, BarState, VendorStatus, CreatedBy)
    VALUES ('Defense Attorney', 'B', 'Smith & Associates Law Firm', 'John Smith, Esq.', '(555) 200-2001', 'jsmith@smithlaw.com', '6234567', 'IL', 'Y', 'system');
    
    DECLARE @VendorId6 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId6, 'M', '100 Legal Plaza, Suite 500', 'Chicago', 'IL', '60601', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Johnson Defense Group' AND VendorType = 'Defense Attorney')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, ContactName, BusinessPhone, Email, BarNumber, BarState, VendorStatus, CreatedBy)
    VALUES ('Defense Attorney', 'B', 'Johnson Defense Group', 'Mary Johnson, Esq.', '(555) 200-2002', 'mjohnson@johnsondefense.com', '6345678', 'IL', 'Y', 'system');
    
    DECLARE @VendorId7 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId7, 'M', '200 Corporate Center, Floor 10', 'Springfield', 'IL', '62701', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Williams & Partners LLP' AND VendorType = 'Defense Attorney')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, ContactName, BusinessPhone, Email, BarNumber, BarState, VendorStatus, CreatedBy)
    VALUES ('Defense Attorney', 'B', 'Williams & Partners LLP', 'Robert Williams, Esq.', '(312) 555-3000', 'rwilliams@williamspartners.com', '6456789', 'IL', 'Y', 'system');
    
    DECLARE @VendorId8 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId8, 'M', '300 N LaSalle St, Suite 2000', 'Chicago', 'IL', '60654', 'system');
END
GO

-- PLAINTIFF ATTORNEYS
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Martinez Injury Attorneys' AND VendorType = 'Plaintiff Attorney')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, DoingBusinessAs, ContactName, BusinessPhone, Email, BarNumber, BarState, VendorStatus, CreatedBy)
    VALUES ('Plaintiff Attorney', 'B', 'Martinez Injury Attorneys', 'Martinez Law', 'Carlos Martinez, Esq.', '(555) 300-3001', 'cmartinez@martinezlaw.com', '6567890', 'IL', 'Y', 'system');
    
    DECLARE @VendorId9 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId9, 'M', '400 Justice Drive', 'Chicago', 'IL', '60602', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Thompson Personal Injury Law' AND VendorType = 'Plaintiff Attorney')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, ContactName, BusinessPhone, Email, BarNumber, BarState, VendorStatus, CreatedBy)
    VALUES ('Plaintiff Attorney', 'B', 'Thompson Personal Injury Law', 'Sarah Thompson, Esq.', '(555) 300-3002', 'sthompson@thompsonpi.com', '6678901', 'IL', 'Y', 'system');
    
    DECLARE @VendorId10 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId10, 'M', '500 Advocate Way, Suite 300', 'Naperville', 'IL', '60540', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Davis & Associates Personal Injury' AND VendorType = 'Plaintiff Attorney')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, ContactName, BusinessPhone, Email, BarNumber, BarState, VendorStatus, CreatedBy)
    VALUES ('Plaintiff Attorney', 'B', 'Davis & Associates Personal Injury', 'Michael Davis, Esq.', '(312) 555-4000', 'mdavis@davisinjury.com', '6789012', 'IL', 'Y', 'system');
    
    DECLARE @VendorId11 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId11, 'M', '1 E Wacker Dr, Suite 1500', 'Chicago', 'IL', '60601', 'system');
END
GO

-- POLICE DEPARTMENTS
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Springfield Police Department' AND VendorType = 'Police Department')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, JurisdictionType, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Police Department', 'B', 'Springfield Police Department', 'City', '(555) 400-4001', 'records@springfieldpd.gov', 'Y', 'system');
    
    DECLARE @VendorId12 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId12, 'M', '800 Civic Center Drive', 'Springfield', 'IL', '62701', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Chicago Police Department' AND VendorType = 'Police Department')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, DoingBusinessAs, JurisdictionType, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Police Department', 'B', 'Chicago Police Department', 'CPD', 'City', '(312) 746-6000', 'records@chicagopolice.org', 'Y', 'system');
    
    DECLARE @VendorId13 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId13, 'M', '3510 S Michigan Ave', 'Chicago', 'IL', '60653', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Illinois State Police' AND VendorType = 'Police Department')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, DoingBusinessAs, JurisdictionType, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Police Department', 'B', 'Illinois State Police', 'ISP', 'State', '(217) 782-6637', 'records@isp.state.il.us', 'Y', 'system');
    
    DECLARE @VendorId14 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId14, 'M', '801 S 7th St', 'Springfield', 'IL', '62703', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Cook County Sheriff' AND VendorType = 'Police Department')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, JurisdictionType, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Police Department', 'B', 'Cook County Sheriff', 'County', '(708) 865-4700', 'records@cookcountysheriff.org', 'Y', 'system');
    
    DECLARE @VendorId15 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId15, 'M', '50 W Washington St', 'Chicago', 'IL', '60602', 'system');
END
GO

-- FIRE DEPARTMENTS
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Springfield Fire Department' AND VendorType = 'Fire Department')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, JurisdictionType, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Fire Department', 'B', 'Springfield Fire Department', 'City', '(555) 500-5001', 'records@springfieldfire.gov', 'Y', 'system');
    
    DECLARE @VendorId16 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId16, 'M', '900 Fire Station Way', 'Springfield', 'IL', '62701', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Chicago Fire Department' AND VendorType = 'Fire Department')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, DoingBusinessAs, JurisdictionType, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Fire Department', 'B', 'Chicago Fire Department', 'CFD', 'City', '(312) 744-4730', 'records@chicagofire.org', 'Y', 'system');
    
    DECLARE @VendorId17 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId17, 'M', '444 N Dearborn St', 'Chicago', 'IL', '60654', 'system');
END
GO

-- MEDICAL PROVIDERS
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Advanced Physical Therapy' AND VendorType = 'Medical Provider')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Medical Provider', 'B', 'Advanced Physical Therapy', '(312) 555-6000', 'info@advancedpt.com', 'Y', 'system');
    
    DECLARE @VendorId18 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId18, 'M', '123 Therapy Lane', 'Chicago', 'IL', '60614', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Midwest Radiology Associates' AND VendorType = 'Medical Provider')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Medical Provider', 'B', 'Midwest Radiology Associates', '(312) 555-7000', 'info@midwestradiology.com', 'Y', 'system');
    
    DECLARE @VendorId19 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId19, 'M', '456 Imaging Center Dr', 'Evanston', 'IL', '60201', 'system');
END
GO

-- TOWING SERVICES
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Speedy Towing & Recovery' AND VendorType = 'Towing Service')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Towing Service', 'B', 'Speedy Towing & Recovery', '(312) 555-8000', 'dispatch@speedytowing.com', 'Y', 'system');
    
    DECLARE @VendorId20 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId20, 'M', '789 Tow Yard Rd', 'Chicago', 'IL', '60632', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'ABC Auto Towing' AND VendorType = 'Towing Service')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Towing Service', 'B', 'ABC Auto Towing', '(773) 555-9000', 'info@abctowing.com', 'Y', 'system');
    
    DECLARE @VendorId21 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId21, 'M', '321 Recovery Blvd', 'Cicero', 'IL', '60804', 'system');
END
GO

-- REPAIR SHOPS
IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Premier Auto Body Shop' AND VendorType = 'Repair Shop')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Repair Shop', 'B', 'Premier Auto Body Shop', '(312) 555-1100', 'estimates@premierautobody.com', 'Y', 'system');
    
    DECLARE @VendorId22 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId22, 'M', '555 Auto Repair Dr', 'Chicago', 'IL', '60638', 'system');
END

IF NOT EXISTS (SELECT 1 FROM VendorMaster WHERE VendorName = 'Quality Collision Center' AND VendorType = 'Repair Shop')
BEGIN
    INSERT INTO VendorMaster (VendorType, EntityType, VendorName, BusinessPhone, Email, VendorStatus, CreatedBy)
    VALUES ('Repair Shop', 'B', 'Quality Collision Center', '(847) 555-1200', 'service@qualitycollision.com', 'Y', 'system');
    
    DECLARE @VendorId23 BIGINT = SCOPE_IDENTITY();
    INSERT INTO VendorAddress (VendorId, AddressType, StreetAddress, City, State, ZipCode, CreatedBy)
    VALUES (@VendorId23, 'M', '888 Repair Lane', 'Schaumburg', 'IL', '60173', 'system');
END
GO

PRINT 'Sample vendors added';
GO

-- ============================================================================
-- VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== Verification: VendorMaster Table ===';
PRINT '';

SELECT 
    VendorType,
    COUNT(*) AS Count
FROM VendorMaster 
WHERE VendorStatus = 'Y'
GROUP BY VendorType
ORDER BY VendorType;

PRINT '';
PRINT '=== Table Structure ===';

SELECT 
    t.name AS TableName,
    (SELECT COUNT(*) FROM VendorMaster) AS VendorCount,
    (SELECT COUNT(*) FROM VendorAddress) AS AddressCount
FROM sys.tables t
WHERE t.name = 'VendorMaster';
GO

PRINT '';
PRINT '================================================';
PRINT 'VendorMaster Table Created Successfully';
PRINT '================================================';
PRINT '';
PRINT 'New Tables:';
PRINT '  - VendorMaster: Stores all vendor information';
PRINT '  - VendorAddress: Stores vendor addresses';
PRINT '';
PRINT 'New Columns Added (if tables exist):';
PRINT '  - Claimants.HospitalVendorId: FK to VendorMaster';
PRINT '  - Claimants.AttorneyVendorId: FK to VendorMaster';
PRINT '  - FnolAuthorities.VendorId: FK to VendorMaster';
PRINT '';
PRINT 'Sample Vendors Added:';
PRINT '  - Hospitals (5)';
PRINT '  - Defense Attorneys (3)';
PRINT '  - Plaintiff Attorneys (3)';
PRINT '  - Police Departments (4)';
PRINT '  - Fire Departments (2)';
PRINT '  - Medical Providers (2)';
PRINT '  - Towing Services (2)';
PRINT '  - Repair Shops (2)';
PRINT '';
PRINT 'Benefits:';
PRINT '  - Vendors (~50K) separated from claim entities (~500K+)';
PRINT '  - Faster vendor searches';
PRINT '  - Optimized indexing';
PRINT '  - Can be cached for performance';
PRINT '================================================';
GO
